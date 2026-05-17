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
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Module.GameOperate;
using Newtonsoft.Json.Linq;

namespace KodakkuScript
{
	[ScriptType(name: "Zelenia Extreme", territorys: [], $129d93a48-e8bd-48c6-aff2-f6b308fdc26e", version: "0.0.0.1", Author: "Linoa235", guid: "e0ed5376-9fa9-4897-9ced-05c707c17682")]

	public class Recollection
	{
		const string noteStr =
	"""
        Game8 strategy, plug and play

        """;
		[UserSetting("Enable Debug Output")]
		public bool EnableDev { get; set; }

		string debugOutput = "";

		int parse = -1;

		List<int> P1Tower = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P2Tether = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_3Mark = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_3Index = [];
		List<int> P3Circle = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_4Mark = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> P3_6Mark = [0, 0, 0, 0, 0, 0, 0, 0];

		bool THFirst = false;
		bool Map = false;
		bool Map2 = false;
		bool Map4 = false;
		bool Map6 = false;
		int P3_5Safe = 0;
		float P3_3North = float.Pi;

		//Near cleave = 0, Far cleave = 1
		List<bool> NearFarCleaveRecord = [false, false, false, false];
		int NearFarCleaveCount = 0;
		Vector3 CloseBase = new(100f, 0f, 94f);
		Vector3 FarBase = new(100f, 0f, 91f);
		Vector3 centre = new(100f, 0f, 100f);

		Vector3 EastTopInner = new(107.5f, 0f, 99.5f);
		Vector3 EastBottomInner = new(107.5f, 0f, 100.5f);
		Vector3 EastTopOuter = new(108.5f, 0f, 99.5f);
		Vector3 EastBottomOuter = new(108.5f, 0f, 100.5f);

		Vector3 WestTopInner = new(92.5f, 0f, 99.5f);
		Vector3 WestBottomInner = new(92.5f, 0f, 100.5f);
		Vector3 WestTopOuter = new(91.5f, 0f, 99.5f);
		Vector3 WestBottomOuter = new(91.5f, 0f, 100.5f);

		Vector3 SouthLeftInner = new(99.5f, 0f, 107.5f);
		//Vector3 SouthRightInner = new(100.5f, 0f, 107.5f);
		Vector3 SouthLeftOuter = new(99.5f, 0f, 108.5f);
		//Vector3 SouthRightOuter = new(100.5f, 0f, 108.5f);

		//Vector3 NorthLeftInner = new(99.5f, 0f, 92.5f);
		Vector3 NorthRightInner = new(100.5f, 0f, 92.5f);
		//Vector3 NorthLeftOuter = new(99.5f, 0f, 91.5f);
		Vector3 NorthRightOuter = new(100.5f, 0f, 91.5f);

		float CloseMulti67_5 = 4.15746f;
		float CloseMulti22_5 = 1.72208f;
		float FarMulti67_5 = 6.46716f;
		float FarMulti22_5 = 2.67878f;


		public void Init(ScriptAccessory accessory)
		{	
			accessory.Method.RemoveDraw(".*");
			debugOutput = "";
			parse = 1;
			P1Tower = [0, 0, 0, 0, 0, 0, 0, 0];
			P2Tether = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_3Mark = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_3Index = [];
			P3Circle = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_4Mark = [0, 0, 0, 0, 0, 0, 0, 0];
			P3_6Mark = [0, 0, 0, 0, 0, 0, 0, 0];

			THFirst = false;
			Map = false;
			Map2 = false;
			Map4 = false;
			Map6 = false;
			P3_5Safe = 0;
			NearFarCleaveRecord = [false, false, false, false];
			NearFarCleaveCount = 0;
			P3_3North = float.Pi;
		}

		[ScriptMethod(name: "Opening_DonutTower_MarkerRecord", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0244"], userControl: false)]
		public void Opening_DonutTower_MarkerRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(((uint)tid));
			P1Tower[tIndex] = 1;
		}

		[ScriptMethod(name: "Opening_DonutTower_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43226"])]
		public void Opening_DonutTower_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P1Tower[myIndex] != 1) return;

			Vector3 TowerNW = new Vector3(92, 0, 95);
			Vector3 TowerNE = new Vector3(108, 0, 95);
			Vector3 TowerSE = new Vector3(108, 0, 105);
			Vector3 TowerSW = new Vector3(92, 0, 105);

			if (myIndex == 0 || myIndex == 6)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Opening_DonutTower_Navigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerNW;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 7)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Opening_DonutTower_Navigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerNE;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 5)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Opening_DonutTower_Navigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerSE;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 4)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Opening_DonutTower_Navigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = TowerSW;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 7000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "FirstRound_NearFarCleave_Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
		public void FirstRound_NearFarCleave_Record(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			if (@event.StatusParam == 759)
			{
				NearFarCleaveRecord[NearFarCleaveCount] = true;
			}
			NearFarCleaveCount++;
			if (EnableDev)
			{
				debugOutput = @event.StatusParam == 759 ? "Far Cleave" : "Near Cleave";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "FirstRound_NearFarCleave_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"])]
		public async void FirstRound_NearFarCleave_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//13-3-3-3
			//Starting from west, clockwise 1-8
			List<Vector3> ClosePos = [];
			List<Vector3> FarPos = [];

			ClosePos.Add(new Vector3(100 - CloseMulti22_5, 0, 100 - CloseMulti67_5));
			ClosePos.Add(new Vector3(100 + CloseMulti22_5, 0, 100 - CloseMulti67_5));
			ClosePos.Add(new Vector3(100 - CloseMulti67_5, 0, 100 - CloseMulti22_5));
			ClosePos.Add(new Vector3(100 + CloseMulti67_5, 0, 100 - CloseMulti22_5));
			ClosePos.Add(new Vector3(100 - CloseMulti22_5, 0, 100 + CloseMulti67_5));
			ClosePos.Add(new Vector3(100 + CloseMulti22_5, 0, 100 + CloseMulti67_5));
			ClosePos.Add(new Vector3(100 - CloseMulti67_5, 0, 100 + CloseMulti22_5));
			ClosePos.Add(new Vector3(100 + CloseMulti67_5, 0, 100 + CloseMulti22_5));

			FarPos.Add(new Vector3(100 - FarMulti22_5, 0, 100 - FarMulti67_5));
			FarPos.Add(new Vector3(100 + FarMulti22_5, 0, 100 - FarMulti67_5));
			FarPos.Add(new Vector3(100 - FarMulti67_5, 0, 100 - FarMulti22_5));
			FarPos.Add(new Vector3(100 + FarMulti67_5, 0, 100 - FarMulti22_5));
			FarPos.Add(new Vector3(100 - FarMulti22_5, 0, 100 + FarMulti67_5));
			FarPos.Add(new Vector3(100 + FarMulti22_5, 0, 100 + FarMulti67_5));
			FarPos.Add(new Vector3(100 - FarMulti67_5, 0, 100 + FarMulti22_5));
			FarPos.Add(new Vector3(100 + FarMulti67_5, 0, 100 + FarMulti22_5));

			await Task.Delay(1000);
			if (NearFarCleaveRecord.Count == 0)
			{
				debugOutput = "Error occurred, please report to DC";
				accessory.Method.SendChat($"""/e {debugOutput}""");
				return;
			}


			if (myIndex < 4)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000); 
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "FirstRound_NearFarCleave_Navigation4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}

		}

		[ScriptMethod(name: "HolyShieldPhase_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43189"], userControl: false)]
		public void HolyShieldPhase_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 2;
			NearFarCleaveRecord = [false, false, false, false];
			NearFarCleaveCount = 0;
		}

		[ScriptMethod(name: "HolyShield_TetherCollection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0011"], userControl: false)]
		public void HolyShield_TetherCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P2Tether[tIndex] = 1;
			if (EnableDev)
			{
				var c = accessory.Data.Objects.SearchById(tid);
				if (c == null) return;				
				debugOutput = c.Name.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "HolyShield_TetherNavigation", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(321[67])$"])]
		public async void HolyShield_TetherNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["Id"], out var id)) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

			Vector3 TH_N_Pos = new(pos.X, 0, pos.Z - 3);
			Vector3 TH_S_Pos = new(pos.X, 0, pos.Z + 3);
			Vector3 DPS_N_Pos = new(pos.X, 0, pos.Z - 3);
			Vector3 DPS_S_Pos = new(pos.X, 0, pos.Z + 3);
			//4561-appear, 3216-right cleave-12822, 3217-left cleave-12823
			await Task.Delay(1000);
			
			if (P2Tether[myIndex] != 1) return;		

			if (EnableDev)
			{
				debugOutput = id.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			if (myIndex < 4)
			{
				//TH group
				if (pos.X > 105f) return;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "HolyShield_TetherNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = id == 12822 ? TH_S_Pos : TH_N_Pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				//DPS group
				if (pos.X < 95f) return;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "HolyShield_TetherNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = id == 12822 ? DPS_N_Pos : DPS_S_Pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "HolyShield_TetherClear", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43187"], userControl: false)]
		public void HolyShield_TetherClear(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			P2Tether = [0, 0, 0, 0, 0, 0, 0, 0];
		}

		[ScriptMethod(name: "HolyShield_TowerNavigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43068"])]
		public void HolyShield_TowerNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P2Tether[myIndex] == 1) return;
			var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
			if (myIndex < 4)
			{
				//TH group
				if (pos.X > 100) return;				
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "HolyShield_TowerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				//DPS group
				if (pos.X < 100) return;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "HolyShield_TowerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = pos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "SecondHalf_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43213"], userControl: false)]
		public void SecondHalf_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 3;
		}

		[ScriptMethod(name: "MagicCircleUnfold_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43193"], userControl: false)]
		public void MagicCircleUnfold_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			if (parse == 3) parse = 4;
			if (parse == 9) parse = 10;
		}

		[ScriptMethod(name: "MagicCircleUnfold_FloorCollection", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void MagicCircleUnfold_FloorCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4 && parse != 10) return;
			if (!int.TryParse(@event["Index"], out var index))return;
			if (index == 6)
			{
				Map = true;
			}
			if (index == 4)
			{
				Map = false;
			}
			if (EnableDev)
			{
				debugOutput = index.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			//04-bottom right start clockwise-false, 06-top start counterclockwise-true
		}

		[ScriptMethod(name: "MagicCircleUnfold_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43198"])]
		public void MagicCircleUnfold_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4 && parse != 10) return;
			Vector3 NorthStart = new(99.2f, 0, 94.8f);
			Vector3 NorthEnd = new(90f, 0, 105.5f);
			Vector3 SouthStart = new(105.6f, 0, 102f);
			Vector3 SouthEnd = new(96.4f, 0, 105.5f);
			if (Map)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_Navigation1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NorthStart;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_Navigation2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NorthEnd;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			else
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_Navigation1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = SouthStart;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_Navigation2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = SouthEnd;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "MagicCircleUnfold_SecondForm_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43540"], userControl: false)]
		public void MagicCircleUnfold_SecondForm_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 5;
		}

		[ScriptMethod(name: "MagicCircleUnfold_SecondForm_FloorCollection", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void MagicCircleUnfold_SecondForm_FloorCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 5) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index == 5)
			{
				Map2 = true;
			}
			if (index == 10)
			{
				Map2 = false;
			}
			if (EnableDev && (index == 5 || index == 10))
			{
				debugOutput = Map2 ? "Heavy left, Donut right" : "Heavy right, Donut left";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			//Get: right-up inner red (05) / left-up inner red (0A)
			//10-Heavy right Donut left-false, 5-Heavy left Donut right-true
		}

		[ScriptMethod(name: "MagicCircleUnfold_SecondForm_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4344[89])$"])]
		public void MagicCircleUnfold_SecondForm_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 5) return;

			//8-Heavy first, 9-Donut first
			if (@event.ActionId == 43448)
			{
				if (Map2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestBottomOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestTopInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestBottomInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastBottomOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastBottomInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastTopInner;
					dp.Color = accessory.Data.DefaultSafeColor;					
					dp.Delay = 8000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (@event.ActionId == 43449)
			{
				if (Map2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastBottomInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastBottomOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastTopOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 8000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestBottomInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestTopOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SecondForm_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestBottomOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 2000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "MagicCircleUnfold_ThirdForm_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43541"], userControl: false)]
		public void MagicCircleUnfold_ThirdForm_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 6;
		}

		[ScriptMethod(name: "MagicCircleUnfold_ThirdForm_FloorCollection", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void MagicCircleUnfold_ThirdForm_FloorCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index < 12)
			{
				P3_3Index.Add(index);
			}
			//Get: inner circle red (north clockwise 04-0B) 4-11
		}

		[ScriptMethod(name: "MagicCircleUnfold_ThirdForm_FloorCalculation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43195"], userControl: false)]
		public void MagicCircleUnfold_ThirdForm_FloorCalculation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (P3_3Index[0] * P3_3Index[1] == 0)
			{
				return;
			}
			if (P3_3Index[0] * P3_3Index[1] == 40)
			{
				P3_3North = 15 * float.Pi / 8;
			}
			else if(P3_3Index[0] * P3_3Index[1] == 55)
			{
				P3_3North = float.Pi / 8;
			}
			else
			{
				P3_3Index[0] -= 3;
				P3_3Index[1] -= 3;
				P3_3North = (P3_3Index[0] + P3_3Index[1]- 1) * float.Pi / 8;
			}
			if (EnableDev)
			{
				if (P3_3Index[0] * P3_3Index[1] == 40)
				{
					debugOutput = "Tile 8 is north";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				else if (P3_3Index[0] * P3_3Index[1] == 55)
				{
					debugOutput = "Tile 1 is north";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				else
				{
					float debugOutputnum = (P3_3Index[0] + P3_3Index[1]) / 2;
					debugOutput = $"""{debugOutputnum} is north""";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}

				debugOutput = P3_3North.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			//Special: 1+7  2+8    (2n-1)pi/8
			//Get: inner circle red (north clockwise 04-0B) 4-11
		}

		[ScriptMethod(name: "MagicCircleUnfold_ThirdForm_MarkerCollection", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"], userControl: false)]
		public void MagicCircleUnfold_ThirdForm_MarkerCollection(Event @event, ScriptAccessory accessory)
		{			
			if (parse != 6) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P3_3Mark[tIndex] = 1;
		}

		[ScriptMethod(name: "MagicCircleUnfold_ThirdForm_MarkerNavigation", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public void MagicCircleUnfold_ThirdForm_MarkerNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//if (P3_3Mark[myIndex] != 1) return;
			if (tid != accessory.Data.Me) return;

			if (EnableDev)
			{
				debugOutput = "You need to place flower";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			if (myIndex == 0 || myIndex == 6)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 5)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North + float.Pi);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 4)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North + (float.Pi * 5 / 4));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 7)
			{
				var dealpos = RotatePoint(CloseBase, centre, P3_3North + (float.Pi * 3 / 4));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "MagicCircleUnfold_ThirdForm_TowerNavigation", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public async void MagicCircleUnfold_ThirdForm_TowerNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			await Task.Delay(1000);
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P3_3Mark[myIndex] == 1) return;
			if (myIndex == 0 || myIndex == 6)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_TowerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 5)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North + float.Pi);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_TowerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 4)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North + (float.Pi * 3 / 2));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_TowerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 7)
			{
				var dealpos = RotatePoint(FarBase, centre, P3_3North + (float.Pi / 2));

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_ThirdForm_TowerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}

		}

		[ScriptMethod(name: "SecondRound_NearFarCleave_DonutRecord", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0244"], userControl: false)]
		public void SecondRound_NearFarCleave_DonutRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			if (tid == accessory.Data.PartyList[4] ||
				tid == accessory.Data.PartyList[5] ||
				tid == accessory.Data.PartyList[6] ||
				tid == accessory.Data.PartyList[7])
			{
				THFirst = true;
				if (EnableDev)
				{
					debugOutput = "TH bait first";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
			}
			if (tid == accessory.Data.PartyList[0] ||
				tid == accessory.Data.PartyList[1] ||
				tid == accessory.Data.PartyList[2] ||
				tid == accessory.Data.PartyList[3])
			{
				THFirst = false;
				if (EnableDev)
				{
					debugOutput = "DPS bait first";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
			}
		}

		[ScriptMethod(name: "SecondRound_NearFarCleave_Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
		public void SecondRound_NearFarCleave_Record(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			if (@event.StatusParam == 759)
			{
				NearFarCleaveRecord[NearFarCleaveCount] = true;
			}
			NearFarCleaveCount++;
			if (EnableDev)
			{
				debugOutput = @event.StatusParam == 759 ? "Far Cleave" : "Near Cleave";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "SecondRound_NearFarCleave_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"])]
		public async void SecondRound_NearFarCleave_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 6) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//13-3-3-3
			//Starting from west, clockwise 1-8
			List<Vector3> ClosePos = [];
			List<Vector3> FarPos = [];

			ClosePos.Add(new Vector3(100 - CloseMulti22_5, 0, 100 - CloseMulti67_5));
			ClosePos.Add(new Vector3(100 + CloseMulti22_5, 0, 100 - CloseMulti67_5));
			ClosePos.Add(new Vector3(100 - CloseMulti67_5, 0, 100 - CloseMulti22_5));
			ClosePos.Add(new Vector3(100 + CloseMulti67_5, 0, 100 - CloseMulti22_5));
			ClosePos.Add(new Vector3(100 - CloseMulti22_5, 0, 100 + CloseMulti67_5));
			ClosePos.Add(new Vector3(100 + CloseMulti22_5, 0, 100 + CloseMulti67_5));
			ClosePos.Add(new Vector3(100 - CloseMulti67_5, 0, 100 + CloseMulti22_5));
			ClosePos.Add(new Vector3(100 + CloseMulti67_5, 0, 100 + CloseMulti22_5));

			FarPos.Add(new Vector3(100 - FarMulti22_5, 0, 100 - FarMulti67_5));
			FarPos.Add(new Vector3(100 + FarMulti22_5, 0, 100 - FarMulti67_5));
			FarPos.Add(new Vector3(100 - FarMulti67_5, 0, 100 - FarMulti22_5));
			FarPos.Add(new Vector3(100 + FarMulti67_5, 0, 100 - FarMulti22_5));
			FarPos.Add(new Vector3(100 - FarMulti22_5, 0, 100 + FarMulti67_5));
			FarPos.Add(new Vector3(100 + FarMulti22_5, 0, 100 + FarMulti67_5));
			FarPos.Add(new Vector3(100 - FarMulti67_5, 0, 100 + FarMulti22_5));
			FarPos.Add(new Vector3(100 + FarMulti67_5, 0, 100 + FarMulti22_5));

			Vector3 WaitN = new Vector3(100, 0, 94.5f);
			Vector3 WaitS = new Vector3(100, 0, 105.5f);
			await Task.Delay(1000);

			if (NearFarCleaveRecord.Count == 0)
			{
				debugOutput = "Error occurred, please report to DC";
				accessory.Method.SendChat($"""/e {debugOutput}""");
				return;
			}

			if (THFirst)
			{
				if (myIndex < 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[0] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[1] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[2] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[3] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myIndex > 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WaitS;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[1] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[2] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[3] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}

			}
			else
			{
				if (myIndex < 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WaitN;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[1] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[2] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[3] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myIndex > 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[0] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 12000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					await Task.Delay(12000);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[1] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[2] == false ? ClosePos[myIndex] : FarPos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 3000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondRound_NearFarCleave_Navigation4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NearFarCleaveRecord[3] == false ? FarPos[myIndex] : ClosePos[myIndex];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "MagicCircleUnfold_FourthForm_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43542"], userControl: false)]
		public void MagicCircleUnfold_FourthForm_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 7;
			NearFarCleaveRecord = [false, false, false, false];
			NearFarCleaveCount = 0;
		}

		[ScriptMethod(name: "MagicCircleUnfold_FourthForm_FloorCollection", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void MagicCircleUnfold_FourthForm_FloorCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index == 6)
			{
				Map4 = true;
			}
			if (EnableDev)
			{
				if (index == 5)
				{
					debugOutput = "Place flower on south";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				if (index == 6)
				{
					debugOutput = "Place flower on north";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
			}
			//Get: inner circle red (north clockwise 04-0B)
			//6-place flower north-true
		}

		[ScriptMethod(name: "MagicCircleUnfold_FourthForm_MarkerCollection", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"], userControl: false)]
		public void MagicCircleUnfold_FourthForm_MarkerCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P3_4Mark[tIndex] = 1;
		}

		[ScriptMethod(name: "MagicCircleUnfold_FourthForm_MarkerNavigation", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public void MagicCircleUnfold_FourthForm_MarkerNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			if (tid != accessory.Data.Me) return;
			if (EnableDev)
			{
				debugOutput = "You need to place flower";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			var P4RotBase = Map4 ? 0 : float.Pi;
			if (myIndex == 0 || myIndex == 4)
			{
				var dealpos = RotatePoint(CloseBase, centre, P4RotBase + float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_FourthForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1 || myIndex == 5)
			{
				var dealpos = RotatePoint(CloseBase, centre, P4RotBase - float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_FourthForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2 || myIndex == 6)
			{
				var dealpos = RotatePoint(FarBase, centre, P4RotBase + float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_FourthForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3 || myIndex == 7)
			{
				var dealpos = RotatePoint(FarBase, centre, P4RotBase - float.Pi / 8);

				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "MagicCircleUnfold_FourthForm_MarkerNavigation";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = dealpos;
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 6000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "ThirdRound_NearFarCleave_Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
		public void ThirdRound_NearFarCleave_Record(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (@event.StatusParam == 759)
			{
				NearFarCleaveRecord[NearFarCleaveCount] = true;
			}
			NearFarCleaveCount++;
			if (EnableDev)
			{
				debugOutput = @event.StatusParam == 759 ? "Far Cleave" : "Near Cleave";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
		}

		[ScriptMethod(name: "ThirdRound_NearFarCleave_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43181"])]
		public async void ThirdRound_NearFarCleave_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//13-3-3-3
			//Starting from west, clockwise 1-8
			List<Vector3> ClosePos = [];
			List<Vector3> FarPos = [];

			ClosePos.Add(new Vector3(100 - CloseMulti22_5, 0, 100 - CloseMulti67_5));
			ClosePos.Add(new Vector3(100 + CloseMulti22_5, 0, 100 - CloseMulti67_5));
			ClosePos.Add(new Vector3(100 - CloseMulti67_5, 0, 100 - CloseMulti22_5));
			ClosePos.Add(new Vector3(100 + CloseMulti67_5, 0, 100 - CloseMulti22_5));
			ClosePos.Add(new Vector3(100 - CloseMulti22_5, 0, 100 + CloseMulti67_5));
			ClosePos.Add(new Vector3(100 + CloseMulti22_5, 0, 100 + CloseMulti67_5));
			ClosePos.Add(new Vector3(100 - CloseMulti67_5, 0, 100 + CloseMulti22_5));
			ClosePos.Add(new Vector3(100 + CloseMulti67_5, 0, 100 + CloseMulti22_5));

			FarPos.Add(new Vector3(100 - FarMulti22_5, 0, 100 - FarMulti67_5));
			FarPos.Add(new Vector3(100 + FarMulti22_5, 0, 100 - FarMulti67_5));
			FarPos.Add(new Vector3(100 - FarMulti67_5, 0, 100 - FarMulti22_5));
			FarPos.Add(new Vector3(100 + FarMulti67_5, 0, 100 - FarMulti22_5));
			FarPos.Add(new Vector3(100 - FarMulti22_5, 0, 100 + FarMulti67_5));
			FarPos.Add(new Vector3(100 + FarMulti22_5, 0, 100 + FarMulti67_5));
			FarPos.Add(new Vector3(100 - FarMulti67_5, 0, 100 + FarMulti22_5));
			FarPos.Add(new Vector3(100 + FarMulti67_5, 0, 100 + FarMulti22_5));

			await Task.Delay(1000);
			if (NearFarCleaveRecord.Count == 0)
			{
				debugOutput = "Error occurred, please report to DC";
				accessory.Method.SendChat($"""/e {debugOutput}""");
				return;
			}


			if (myIndex < 4)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex > 3)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 12000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				await Task.Delay(12000);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? FarPos[myIndex] : ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation3";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = ClosePos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 3000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "ThirdRound_NearFarCleave_Navigation4";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = NearFarCleaveRecord[0] != NearFarCleaveRecord[1] ? ClosePos[myIndex] : FarPos[myIndex];
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 6000;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}

		}

		[ScriptMethod(name: "OutsideCloneHalfRoomCleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4318[45])$"])]
		public void OutsideCloneHalfRoomCleave(Event @event, ScriptAccessory accessory)
		{
			if (parse != 7) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			if (EnableDev)
			{
				debugOutput = "Watch for left/right cleave";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "OutsideCloneHalfRoomCleave";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = @event["ActionId"] == "43185" ? float.Pi / 2 : float.Pi / -2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 6000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
		}

		[ScriptMethod(name: "MagicCircleUnfold_FifthForm_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43543"], userControl: false)]
		public void MagicCircleUnfold_FifthForm_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 8;
		}

		[ScriptMethod(name: "MagicCircleUnfold_FifthForm_BladeCollection", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4571"], userControl: false)]
		public void MagicCircleUnfold_FifthForm_BladeCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 8) return;
			var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
			if (P3_5Safe == 0)
			{
				if (pos.Z > 100)
				{
					P3_5Safe = 1;
				}
				if (pos.Z < 100)
				{
					P3_5Safe = 2;
				}			
				if (EnableDev)
				{
					if (P3_5Safe == 1)
					{
						debugOutput = "South safe";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
					if (P3_5Safe == 2)
					{
						debugOutput = "North safe";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
					if (P3_5Safe != 1 && P3_5Safe != 2)							
					{
						debugOutput = "Error!";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
				}
			}
			//1-South safe, 2-North safe
		}

		[ScriptMethod(name: "MagicCircleUnfold_FifthForm_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4344[89])$"])]
		public void MagicCircleUnfold_FifthForm_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 8) return;

			//8-Heavy first, 9-Donut first
			if (@event.ActionId == 43448)
			{
				if (P3_5Safe == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastTopOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = SouthLeftInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestBottomOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NorthRightInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (@event.ActionId == 43449)
			{
				if (P3_5Safe == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = EastTopInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = SouthLeftOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = WestBottomInner;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_FifthForm_Navigation2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = NorthRightOuter;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 6000;
					dp.DestoryAt = 3000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "MagicCircleUnfold_SixthForm_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43544"], userControl: false)]
		public void MagicCircleUnfold_SixthForm_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			parse = 9;
		}

		[ScriptMethod(name: "MagicCircleUnfold_SixthForm_FloorCollection", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:256"], userControl: false)]
		public void MagicCircleUnfold_SixthForm_FloorCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (index == 6)
			{
				Map6 = true;
			}
			if (EnableDev)
			{
				debugOutput = index.ToString();
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			//Get: right-up inner red (05) / right-bottom inner red (06)
			//06-true:MTD3 tower near7 flower far8; STD4 flower near1 tower far2; H1D1 flower near5 tower far6; H2D2 tower near3 flower far4
			//05-false:MTD3 flower near8 tower far7; STD4 tower near2 flower far1; H1D1 tower near6 flower far5; H2D2 flower near4 tower far3	
			// (2n-1)*pi/8
		}

		[ScriptMethod(name: "MagicCircleUnfold_SixthForm_MarkerCollection", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"], userControl: false)]
		public void MagicCircleUnfold_SixthForm_MarkerCollection(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var tIndex = accessory.Data.PartyList.IndexOf(tid);
			P3_6Mark[tIndex] = 1;
		}

		[ScriptMethod(name: "MagicCircleUnfold_SixthForm_MarkerNavigation", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0250"])]
		public void MagicCircleUnfold_SixthForm_MarkerNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//if (P3_6Mark[myIndex] != 1) return;
			if (tid != accessory.Data.Me) return;
			if (EnableDev)
			{
				debugOutput = "You need to place flower";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}
			if (myIndex == 0 || myIndex == 6)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 15 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 15 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 1 || myIndex == 7)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 1 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 1 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 2 || myIndex == 4)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 9 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 9 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 3 || myIndex == 5)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 7 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 7 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 6000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

		[ScriptMethod(name: "MagicCircleUnfold_SixthForm_TowerNavigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43201"])]
		public void MagicCircleUnfold_SixthForm_TowerNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 9) return;
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (P3_6Mark[myIndex] == 1) return;
			if (EnableDev)
			{
				debugOutput = "You need to do tower";
				accessory.Method.SendChat($"""/e {debugOutput}""");
			}

			if (myIndex == 0 || myIndex == 6)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 13 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 13 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 1 || myIndex == 7)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 3 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 3 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 2 || myIndex == 4)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 11 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 11 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myIndex == 3 || myIndex == 5)
			{
				if (Map6)
				{
					var dealpos = RotatePoint(CloseBase, centre, float.Pi * 5 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				else
				{
					var dealpos = RotatePoint(FarBase, centre, float.Pi * 5 / 8);

					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "MagicCircleUnfold_SixthForm_MarkerNavigation";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = dealpos;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 8000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}

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
		private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
		{

			Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

			var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
			var lenth = v2.Length();
			return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
		}
/*
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
		}*/
		#endregion
	}
}
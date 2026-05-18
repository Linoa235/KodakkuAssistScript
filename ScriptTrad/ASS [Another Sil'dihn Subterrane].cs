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
using KodakkuAssist.Extensions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace KodakkuScript
{
	[ScriptType(name: "Arcadia Cruiser Zero 1", territorys: [1257], guid: "4cada783-79b1-47b9-9a42-a3a1e2db18da", version: "0.0.0.6", note: noteStr, author: "Linoa235")]

	public class M5S
	{
		const string noteStr =
	"""
        Game8 strategy
        Added CNServer strategy

        """;
		[UserSetting("Enable Debug Output")]
		public bool EnableDev { get; set; }
		
		[UserSetting("Overall Strategy")]
		public static StgEnum GlobalStrat { get; set; } = StgEnum.Game8_Default;
		
		public enum StgEnum
		{
			Game8_Default,
			CnServer,
		}
		
		string debugOutput = "";
		int parse = -1;
		bool DanceFirst = false;
		bool DanceFirst2 = false;
		int LightPos = 0;
		int LightPos1 = 0;
		int LightPos2 = 0;
		int FrogPos2 = 0;
		bool Light2Round = false;
		bool IsNorthSafeInFrog1 = false;
		int SituationInFrog2 = 0;
		int Frog2Round= -1;
		int PairBuffCount = 0;
		List<int> Dance = [0, 0, 0, 0, 0, 0, 0, 0];
		List<int> ABPair = [0, 0, 0, 0, 0, 0, 0, 0];
		bool initHint = false;
		
		public void Init(ScriptAccessory accessory)
		{
			accessory.Method.RemoveDraw(".*");
			debugOutput = "";
			parse = 1;
			DanceFirst = false;
			DanceFirst2 = false;
			LightPos = 0;
			LightPos1 = 0;
			LightPos2 = 0;
			FrogPos2 = 0;
			Light2Round = false;
			IsNorthSafeInFrog1 = false;
			SituationInFrog2 = 0;
			Frog2Round = -1;
			PairBuffCount = 1;
			Dance = [0, 0, 0, 0, 0, 0, 0, 0];
			ABPair = [0, 0, 0, 0, 0, 0, 0, 0];
			initHint = false;
		}
		
		[ScriptMethod(name: "Strategy and Role Hint", eventType: EventTypeEnum.StartCasting,
			eventCondition: ["ActionId:regex:^(42787)$"], userControl: true)]
		public void StrategyAndRoleHint(Event ev, ScriptAccessory sa)
		{
			if (initHint) return;
			initHint = true;
			var myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me); 
			List<string> role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
			sa.Method.TextInfo(
				$"You are [{role[myIndex]}], using strategy [{(GlobalStrat == StgEnum.CnServer ? "CN Server" : "Game8")}],\nPlease adjust if incorrect.", 4000, true);
		}
		
		[ScriptMethod(name: "Heavy/Donut_RangeDisplay", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4287[68])$"])]
		public void HeavyDonut_RangeDisplay(Event @event, ScriptAccessory accessory)
		{
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//42876-Heavy first, 42878-Donut first
			if (@event.ActionId == 42876) 
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Heavy First";
				dp.Scale = new(7);
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Donut Second";
				dp.Scale = new(40);
				dp.InnerScale = new(5);
				dp.Radian = float.Pi * 2;
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.Delay = 5000;
				dp.DestoryAt = 2500;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
			}
			if (@event.ActionId == 42878)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Donut First";
				dp.Scale = new(40);
				dp.InnerScale = new(5);
				dp.Radian = float.Pi * 2;
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

				dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Heavy Second";
				dp.Scale = new(7);
				dp.Owner = sid;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.Delay = 5000;
				dp.DestoryAt = 2500;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
			}

		}

		[ScriptMethod(name: "Dance_DirectionRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4276[2345])$"], userControl: false)]
		public void Dance_DirectionRecord(Event @event, ScriptAccessory accessory)
		{
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//Little frog half-room cleave 4276X  4-east 3-north 5-west 2-south
			//Dance: 0-unknown 1-south 2-east 3-north 4-west
			for (int i = 0; i < 8; i++)
			{
				if (Dance[i] == 0)
				{
					Dance[i] = @event.ActionId switch
					{
						42762 => 1,
						42764 => 2,
						42763 => 3,
						42765 => 4,
						_ => 0,
					};
					return;
				}
			}
		}

		[ScriptMethod(name: "Dance_RecordClear", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4286[34])$"], userControl: false)]
		public void Dance_RecordClear(Event @event, ScriptAccessory accessory)
		{
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			Dance = [0, 0, 0, 0, 0, 0, 0, 0];
		}

		[ScriptMethod(name: "VariousHalfRoomCleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4278[89])|42869|42870)$"])]
		public void VariousHalfRoomCleave(Event @event, ScriptAccessory accessory)
		{
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			if (@event.ActionId == 42788 || @event.ActionId == 42869)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "OutsideCloneHalfRoomCleave";
				dp.Scale = new(80, 80);
				dp.Owner = sid;
				dp.Rotation = float.Pi / -2;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
			}
			if (@event.ActionId == 42789 || @event.ActionId == 42870)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "OutsideCloneHalfRoomCleave";
				dp.Scale = new(80, 80);
				dp.Owner = sid;
				dp.Rotation = float.Pi / 2;
				dp.Color = accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
			}

		}

		[ScriptMethod(name: "Appointment_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4220[345678])|(4279[234567])|(4280[0123459])|(4281[01234]))$"])]
		public void Appointment_Navigation(Event @event, ScriptAccessory accessory)
		{
			var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//8.5s delay
			if (myIndex == 0)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Appointment_NavigationMT";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = new Vector3(97f, 0f, 97.5f);
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 8500;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 1)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Appointment_NavigationST";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = new Vector3(97f, 0f, 102.5f);
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 8500;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 2)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Appointment_NavigationH1";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = new Vector3(100f, 0f, 95f);
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 8500;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 3)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Appointment_NavigationH2";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = new Vector3(100f, 0f, 105f);
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 8500;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 4 || myIndex == 5)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Appointment_NavigationMelee";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = new Vector3(103f, 0f, 102.5f);
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 8500;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
			if (myIndex == 6 || myIndex == 7)
			{
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Appointment_NavigationRanged";
				dp.Scale = new(2);
				dp.ScaleMode |= ScaleMode.YByDistance;
				dp.Owner = accessory.Data.Me;
				dp.TargetPosition = new Vector3(103f, 0f, 97.5f);
				dp.Color = accessory.Data.DefaultSafeColor;
				dp.Delay = 8500;
				dp.DestoryAt = 3000;
				accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
			}
		}

		[ScriptMethod(name: "Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42846)$"])]
		public void Spread(Event @event, ScriptAccessory accessory)
		{
			//42844 = Stack, 42846 = Spread
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Spread";
			dp.Scale = new(5);
			dp.Owner = tid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 5000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);			
		}

		[ScriptMethod(name: "Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42844)$"])]
		public void Stack(Event @event, ScriptAccessory accessory)
		{
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;

			//42844 = Stack, 42846 = Spread
			if (parse == 2 || parse == 3)
			{
				int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
				var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
				var i = accessory.Data.PartyList.IndexOf(tid);
				var isMyGroup = myindex == i || group[i] == myindex;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Stack";
				dp.Scale = new(4);
				dp.Owner = tid == accessory.Data.Me ? accessory.Data.PartyList[group[myindex]] : tid;
				dp.Color = (group[myindex] == accessory.Data.PartyList.IndexOf(tid) || tid == accessory.Data.Me) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
			}

			if (parse == 4)
			{
				int[] group = [6, 5, 4, 7, 2, 1, 0, 3];
				var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
				var i = accessory.Data.PartyList.IndexOf(tid);
				var isMyGroup = myindex == i || group[i] == myindex;
				var dp = accessory.Data.GetDefaultDrawProperties();
				dp.Name = "Stack";
				dp.Scale = new(4);
				dp.Owner = tid == accessory.Data.Me ? accessory.Data.PartyList[group[myindex]] : tid;
				dp.Color = (group[myindex] == accessory.Data.PartyList.IndexOf(tid) || tid == accessory.Data.Me) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
				dp.DestoryAt = 5000;
				accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
			}
		}

		[ScriptMethod(name: "FirstSpotlight_DanceBuffRecord",	eventType: EventTypeEnum.StatusAdd,	eventCondition: ["StatusID:4461"],	userControl: false)]
		public void FirstSpotlight_DanceBuffRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myindex != accessory.Data.PartyList.IndexOf(tid)) return;
			var time = JsonConvert.DeserializeObject<double>(@event["Duration"]);

			if (time < 28.0)
			{
				DanceFirst = true;
			}
		}

		[ScriptMethod(name: "FirstSpotlight_FloorRecord", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:regex:^(2)|(32)$"], userControl: false)]
		public void FirstSpotlight_FloorRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			if (!int.TryParse(@event["Index"], out var index)) return;
			if (!int.TryParse(@event["Flag"], out var flag)) return;
			//LightPos 0-unknown 1-northwest safe 2-northeast safe
			//flag32=bottom-left top-right, flag2=top-left bottom-right
			if (LightPos != 0) return;
			if (index != 3) return;
			if (flag == 2)
			{
				LightPos = 1;
				if (EnableDev)
				{
					debugOutput = "Top-left bottom-right";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				return;
			}
			if (flag == 32)
			{
				LightPos = 2;
				if (EnableDev)
				{
					debugOutput = "Bottom-left top-right";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}
				return;
			}
		}

		[ScriptMethod(name: "FirstSpotlight_LampInitialPositionRecord", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:18363"], userControl: false)]
		public void FirstSpotlight_LampInitialPositionRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			//LightPos1 Check Boss's near north/south lamp 0-unknown 1-right lamp 2-left lamp 
			if (LightPos1 != 0) return;
			if (@event.SourcePosition.Z > 107f && @event.SourcePosition.Z < 108f)
			{
				if (@event.SourcePosition.X > 102f && @event.SourcePosition.X < 103f)
				{
					LightPos1 = 1;
					if (EnableDev)
					{
						debugOutput = "Right lamp";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}

					return;
				}
				if (@event.SourcePosition.X > 97f && @event.SourcePosition.X < 98f)
				{
					LightPos1 = 2;
					if (EnableDev)
					{
						debugOutput = "Left lamp";
						accessory.Method.SendChat($"""/e {debugOutput}""");
					}
					return;
				}
			}
		}

		[ScriptMethod(name: "FirstSpotlight_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42834)$"])]
		public void FirstSpotlight_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 1) return;
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			//22 30			
			//1-northwest safe 2-northeast safe
			if (myindex == 0 || myindex == 4)
			{
				if (LightPos == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationMTD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst ? (LightPos1 == 1 ? new Vector3(97.5f, 0f, 107.5f) : new Vector3(92.5f, 0f, 102.5f)) : (LightPos1 == 2 ? new Vector3(97.5f, 0f, 107.5f) : new Vector3(92.5f, 0f, 102.5f));
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, bottom-left near lamp", 5000);
				}
				if (LightPos == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationMTD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst ? (LightPos1 == 2 ? new Vector3(97.5f, 0f, 92.5f) : new Vector3(92.5f, 0f, 97.5f)): (LightPos1 == 1 ? new Vector3(97.5f, 0f, 92.5f) : new Vector3(92.5f, 0f, 97.5f));
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, top-left near lamp", 5000);
				}
			}
			if (myindex == 1 || myindex == 5)
			{
				if (LightPos == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst ? (LightPos1 == 2 ? new Vector3(107.5f, 0f, 97.5f) : new Vector3(102.5f, 0f, 92.5f)): (LightPos1 == 1 ? new Vector3(107.5f, 0f, 97.5f) : new Vector3(102.5f, 0f, 92.5f));
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, top-right near lamp", 5000);
				}
				if (LightPos == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst ? (LightPos1 == 2 ? new Vector3(102.5f, 0f, 107.5f) : new Vector3(107.5f, 0f, 102.5f)): (LightPos1 == 1 ? new Vector3(102.5f, 0f, 107.5f) : new Vector3(107.5f, 0f, 102.5f));
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, bottom-right near lamp", 5000);
				}
			}
			if (myindex == 2 || myindex == 6)
			{
				if (LightPos == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationH1D3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = new Vector3(87.5f, 0f, 87.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, top-left far lamp", 5000);
				}
				if (LightPos == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationH1D3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = new Vector3(87.5f, 0f, 112.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, bottom-left far lamp", 5000);
				}
			}
			if (myindex == 3 || myindex == 7)
			{
				if (LightPos == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = new Vector3(112.5f, 0f, 112.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, bottom-right far lamp", 5000);
				}
				if (LightPos == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "FirstSpotlightNavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = new Vector3(112.5f, 0f, 87.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = DanceFirst ? 18000 : 26000;
					dp.DestoryAt = 4000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
					accessory.Method.TextInfo($"Soon, top-right far lamp", 5000);
				}
			}
		}

		[ScriptMethod(name: "Dance1_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42848)$"], userControl: false)]
		public void Dance1_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			if (parse == 1) parse = 2;
		}

		[ScriptMethod(name: "Dance1_HeavyDonut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(39908)$"])]
		public void Dance1_HeavyDonut(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy1";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 5200;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut1";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 5200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy2";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 7700;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut2";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 10200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy3";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 12700;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut3";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 15200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy4";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 17700;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut4";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 20200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
		}

		[ScriptMethod(name: "Dance1_Cone", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(42852)$"])]
		public void Dance1_Cone(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance1_Cone";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Owner = sid;
			dp.TargetPosition = @event.TargetPosition;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
		}

		[ScriptMethod(name: "Dance1_BuffRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(42852)$"], userControl: false)]
		public void Dance1_BuffRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var i = accessory.Data.PartyList.IndexOf(tid);
			ABPair[i] = PairBuffCount;
			PairBuffCount++;
		}

		[ScriptMethod(name: "Dance1_PairPartner", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
		public void Dance1_PairPartner(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			for (int i = 0; i < 8; i++)
			{
				if (ABPair[i] + ABPair[myindex] == 9)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "Dance1_PairPartner";
					dp.Scale = new(2);
					dp.Owner = accessory.Data.PartyList[i];
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 30000;
					accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
				}
			}
		}

		[ScriptMethod(name: "Dance1_PositionNavigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
		public void Dance1_PositionNavigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			var myObj = accessory.Data.MyObject;
			if (myObj == null) return;
			var myStatus = myObj.HasStatus(4462) ? 4462u : 4463u;
			var myStatusTime = GetStatusRemainingTime(accessory, (IBattleChara?)myObj, myStatus);
			accessory.Log.Debug($"Status remaining time: {myStatusTime}");
			var myPos = 0;
			if (myStatusTime > 7) myPos++;
			if (myStatusTime > 12) myPos++;
			if (myStatusTime > 17) myPos++;
			if (myStatusTime > 22) myPos++;
			// idx 0 cannot occur unless no buff at all
			// From top to bottom based on buff length
			List<Vector3> safePos = [new(100, 0, 100), new(100, 0, 92.5f), new(100, 0, 97.5f), new(100, 0, 102.5f), new(100, 0, 107.5f)];

			// Navigation
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance1_PositionNavigation";
			dp.Scale = new(2);
			dp.ScaleMode |= ScaleMode.YByDistance;
			dp.Owner = accessory.Data.Me;
			dp.TargetPosition = safePos[myPos];
			dp.Color = accessory.Data.DefaultSafeColor;
			dp.DestoryAt = 5000;
			accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
		}
		
		[ScriptMethod(name: "Dance1_PairPartnerClear", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(42856|(3947[568]))$"])]
		public void Dance1_PairPartnerClear(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myindex != accessory.Data.PartyList.IndexOf(tid)) return;
			else accessory.Method.RemoveDraw($"Dance1_PairPartner");
		}

		[ScriptMethod(name: "Dance1_LeftRightRangeDisplay", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
		public void Dance1_LeftRightRangeDisplay(Event @event, ScriptAccessory accessory)
		{
			if (parse != 2) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//Resolution First 42858 5.8s Second 41872 5.8s +2.5 each
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance1";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[0] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 6000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance2";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[1] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance3";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[2] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 8500;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance4";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[3] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 11000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance5";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[4] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 13500;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance6";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[5] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 16000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance7";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[6] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 18500;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance8";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[7] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 21000;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
		}

		[ScriptMethod(name: "LittleFrog1_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42867)$"], userControl: false)]
		public void LittleFrog1_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			if (parse == 2) parse = 3;
		}

		[ScriptMethod(name: "LittleFrog1_SafeZoneRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42867)$"], userControl: false)]
		public void LittleFrog1_SafeZoneRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3) return;
			if ((@event.SourcePosition.X -100)*(@event.SourcePosition.Z - 100) > 0)
			{
				IsNorthSafeInFrog1 = true;
			}
		}

		[ScriptMethod(name: "LittleFrog1_NavigationG8", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4284[46])$"])]
		public void LittleFrog1_NavigationG8(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3) return;
			if (GlobalStrat != StgEnum.Game8_Default) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			int[] group = [6, 5, 4, 7, 2, 1, 0, 3];
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myindex != accessory.Data.PartyList.IndexOf(tid) && group[myindex] != accessory.Data.PartyList.IndexOf(tid)) return;
			//42844 = Stack, 42846 = Spread
			if (@event.ActionId == 42844)
			{
				if (myindex == 0 || myindex == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_StackNavigationMTD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1? new Vector3(100f, 0f, 89.5f): new Vector3(89.5f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 1 || myindex == 5)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_StackNavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(100f, 0f, 110.5f) : new Vector3(110.5f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 2 || myindex == 6)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_StackNavigationH1D3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(100f, 0f, 81.5f) : new Vector3(81.5f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 3 || myindex == 7)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_StackNavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(100f, 0f, 118.5f) : new Vector3(118.5f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (@event.ActionId == 42846)
			{
				if (myindex == 0)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationMT";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(104.5f, 0f, 89.5f) : new Vector3(89.5f, 0f, 95.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationST";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(95.5f, 0f, 110.5f) : new Vector3(110.5f, 0f, 104.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationH1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(104.5f, 0f, 81.5f) : new Vector3(81.5f, 0f, 104.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationH2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(95.5f, 0f, 118.5f) : new Vector3(118.5f, 0f, 104.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(95.5f, 0f, 89.5f) : new Vector3(89.5f, 0f, 104.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 5)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(104.5f, 0f, 110.5f) : new Vector3(110.5f, 0f, 95.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 6)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(95.5f, 0f, 81.5f) : new Vector3(81.5f, 0f, 104.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (myindex == 7)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog1_SpreadNavigationD4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = IsNorthSafeInFrog1 ? new Vector3(104.5f, 0f, 118.5f) : new Vector3(118.5f, 0f, 95.5f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 5000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
		}
		
		[ScriptMethod(name: "LittleFrog1_NavigationCN", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4284[46])$"])]
		public void LittleFrog1_NavigationCN(Event @event, ScriptAccessory accessory)
		{
			// Dragon Dragon Phoenix Phoenix
			if (parse != 3) return;
			if (GlobalStrat != StgEnum.CnServer) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			int[] group = [6, 5, 4, 7, 2, 1, 0, 3];
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myindex != accessory.Data.PartyList.IndexOf(tid) && group[myindex] != accessory.Data.PartyList.IndexOf(tid)) return;
			//42844 = Stack, 42846 = Spread
			var isSpread = @event.ActionId == 42846;

			List<Vector3> safePoints = Enumerable.Repeat(new Vector3(0, 0, 0), 16).ToList();
		
			safePoints[0] = new Vector3(95.5f, 0, 89.5f);
			safePoints[1] = FoldPointHorizon(safePoints[0], 100);
			safePoints[6] = safePoints[0] - new Vector3(0, 0, 8);			// 0
			safePoints[7] = FoldPointHorizon(safePoints[6], 100);	// 1
		
			safePoints[2] = FoldPointVertical(safePoints[6], 100);	// 4
			safePoints[3] = FoldPointVertical(safePoints[7], 100);	// 5
			safePoints[4] = FoldPointVertical(safePoints[0], 100);
			safePoints[5] = FoldPointVertical(safePoints[1], 100);

			safePoints[8] = new Vector3(safePoints[0].Z, safePoints[0].Y, safePoints[0].X);
			safePoints[9] = FoldPointHorizon(safePoints[8], 100);
			safePoints[14] = safePoints[8] - new Vector3(8, 0, 0);			// 8
			safePoints[15] = FoldPointHorizon(safePoints[14], 100);	// 9
		
			safePoints[10] = FoldPointVertical(safePoints[14], 100);	// 12
			safePoints[11] = FoldPointVertical(safePoints[15], 100);	// 13
			safePoints[12] = FoldPointVertical(safePoints[8], 100);
			safePoints[13] = FoldPointVertical(safePoints[9], 100);

			List<int> stackPosIdx = [0, 1, 4, 5, 4, 5, 0, 1, 8, 9, 12, 13, 12, 13, 8, 9];
			var posIdx = myindex + (IsNorthSafeInFrog1 ? 0 : 8);
			if (!isSpread)
				posIdx = stackPosIdx[posIdx];
		
			accessory.Log.Debug(
				$"Strategy: {(GlobalStrat == StgEnum.CnServer ? "CN Server" : "Game8 Default")}, Player: {myindex}, Safe zone: {(IsNorthSafeInFrog1 ? "Top-Bottom" : "Left-Right")}");

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "LittleFrog1_Navigation";
			dp.Scale = new(2);
			dp.ScaleMode |= ScaleMode.YByDistance;
			dp.Owner = accessory.Data.Me;
			dp.TargetPosition = safePoints[posIdx];
			dp.Color = accessory.Data.DefaultSafeColor;
			dp.DestoryAt = 5000;
			accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

		}

		[ScriptMethod(name: "SecondSpotlight_DanceBuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"], userControl: false)]
		public void SecondSpotlight_DanceBuffRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myindex != accessory.Data.PartyList.IndexOf(tid)) return;
			var time = JsonConvert.DeserializeObject<double>(@event["Duration"]);

			if (time < 13.0)
			{
				DanceFirst2 = true;
			}
		}

		[ScriptMethod(name: "SecondSpotlight_LampInitialPositionRecord", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:18363"], userControl: false)]
		public void SecondSpotlight_LampInitialPositionRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3) return;
			//LightPos 0-unknown 1-corner start (first round edge, second round corner) 2-edge start (first round corner, second round edge) 
			if (LightPos2 != 0) return;
			if (@event.SourcePosition.X == 85f && @event.SourcePosition.Z == 85f)
			{
				LightPos2 = 1;
				if (EnableDev)
				{
					debugOutput = "Lamp initial position at corner";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}

				return;
			}
			if (@event.SourcePosition.X == 85f && @event.SourcePosition.Z == 100f)
			{
				LightPos2 = 2;
				if (EnableDev)
				{
					debugOutput = "Lamp initial position at edge";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}

				return;
			}
		}

		[ScriptMethod(name: "SecondSpotlight_FrogInitialPositionRecord", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:18362"], userControl: false)]
		public void SecondSpotlight_FrogInitialPositionRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3) return;
			//LightPos 0-unknown 1-corner start (first round corner, second round edge) 2-edge start (first round edge, second round corner)
			if (FrogPos2 != 0) return;
			if (@event.SourcePosition.X == 95f && @event.SourcePosition.Z == 95f)
			{
				FrogPos2 = 1;
				if (EnableDev)
				{
					debugOutput = "First round frog at corner";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}

				return;
			}
			if (@event.SourcePosition.X == 100f || @event.SourcePosition.Z == 100f)
			{
				FrogPos2 = 2;
				if (EnableDev)
				{
					debugOutput = "First round frog at edge";
					accessory.Method.SendChat($"""/e {debugOutput}""");
				}

				return;
			}
		}

		[ScriptMethod(name: "SecondSpotlight_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42871)$"])]
		public void SecondSpotlight_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 3) return;
			if (Light2Round) return;
			//Top-left as reference
			Vector3 centre = new(100f, 0f, 100f);
			Vector3 EdgeLamp = new(100f, 0f, 85f);
			Vector3 CornerLamp = new(85f, 0f, 85f);
			Vector3 EdgeLampEdgeFrog = new(98f, 0f, 92f);
			Vector3 EdgeLampCornerFrog = new(94.5f, 0f, 94.5f);
			Vector3 CornerLampEdgeFrog = new(100f, 0f, 92f);
			Vector3 CornerLampCornerFrog = new(95f, 0f, 94.5f);

			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			if (myindex == 0 || myindex == 6)
			{
				if (LightPos2 == 1 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp,centre,0 * float.Pi / 2) : RotatePoint(EdgeLampCornerFrog, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampEdgeFrog, centre, 0 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 1 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 0 * float.Pi / 2) : RotatePoint(EdgeLampEdgeFrog, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampCornerFrog, centre, 0 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 0 * float.Pi / 2) : RotatePoint(CornerLampCornerFrog, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampEdgeFrog, centre, 0 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 0 * float.Pi / 2) : RotatePoint(CornerLampEdgeFrog, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationMTD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampCornerFrog, centre, 0 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 0 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			// Bottom-right, G8 ST, CN H2
			if (myindex == (GlobalStrat == StgEnum.CnServer ? 3: 1) || myindex == 5)
			{
				if (LightPos2 == 1 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 2 * float.Pi / 2) : RotatePoint(EdgeLampCornerFrog, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampEdgeFrog, centre, 2 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 1 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 2 * float.Pi / 2) : RotatePoint(EdgeLampEdgeFrog, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampCornerFrog, centre, 2 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 2 * float.Pi / 2) : RotatePoint(CornerLampCornerFrog, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampEdgeFrog, centre, 2 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 2 * float.Pi / 2) : RotatePoint(CornerLampEdgeFrog, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;	
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationH2D2" : "SecondSpotlight_NavigationSTD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampCornerFrog, centre, 2 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 2 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == 2 || myindex == 4)
			{
				if (LightPos2 == 1 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 3 * float.Pi / 2) : RotatePoint(EdgeLampCornerFrog, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampEdgeFrog, centre, 3 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 1 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 3 * float.Pi / 2) : RotatePoint(EdgeLampEdgeFrog, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampCornerFrog, centre, 3 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 3 * float.Pi / 2) : RotatePoint(CornerLampCornerFrog, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampEdgeFrog, centre, 3 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 3 * float.Pi / 2) : RotatePoint(CornerLampEdgeFrog, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "SecondSpotlight_NavigationH1D1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampCornerFrog, centre, 3 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 3 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			// Top-right, G8 H2, CN ST
			if (myindex == (GlobalStrat == StgEnum.CnServer ? 1 : 3) || myindex == 7)
			{
				if (LightPos2 == 1 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 1 * float.Pi / 2) : RotatePoint(EdgeLampCornerFrog, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampEdgeFrog, centre, 1 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 1 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLamp, centre, 1 * float.Pi / 2) : RotatePoint(EdgeLampEdgeFrog, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLampCornerFrog, centre, 1 * float.Pi / 2) : RotatePoint(CornerLamp, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 1 * float.Pi / 2) : RotatePoint(CornerLampCornerFrog, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampEdgeFrog, centre, 1 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (LightPos2 == 2 && FrogPos2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(CornerLamp, centre, 1 * float.Pi / 2) : RotatePoint(CornerLampEdgeFrog, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

					dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "SecondSpotlight_NavigationSTD4" : "SecondSpotlight_NavigationH2D4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = DanceFirst2 ? RotatePoint(EdgeLampCornerFrog, centre, 1 * float.Pi / 2) : RotatePoint(EdgeLamp, centre, 1 * float.Pi / 2);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.Delay = 9000;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}

			Light2Round = true;
		}

		[ScriptMethod(name: "Dance2_PhaseChange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41840)$"], userControl: false)]
		public void Dance2_PhaseChange(Event @event, ScriptAccessory accessory)
		{
			if (parse == 3) parse = 4;
		}

		[ScriptMethod(name: "Dance1_HeavyDonut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41840)$"])]
		public void Dance2_HeavyDonut(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;

			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy1";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 5200;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut1";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 5200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy2";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 7700;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut2";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 10200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy3";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 12700;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut3";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 15200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Heavy4";
			dp.Scale = new(7);
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 17700;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Donut4";
			dp.Scale = new(40);
			dp.InnerScale = new(5);
			dp.Radian = float.Pi * 2;
			dp.Owner = sid;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 20200;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
		}

		[ScriptMethod(name: "Dance2_Cone", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(42852)$"])]
		public void Dance2_Cone(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			if (!ParseObjectId(@event["TargetId"], out var tid)) return;
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance1_Cone";
			dp.Scale = new(60);
			dp.Radian = float.Pi / 4;
			dp.Owner = sid;
			dp.TargetPosition = @event.TargetPosition;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 2500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
		}

		[ScriptMethod(name: "Dance2_FrontBackLeftRightRangeDisplay", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41872)$"])]
		public void Dance2_FrontBackLeftRightRangeDisplay(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4) return;
			if (!ParseObjectId(@event["SourceId"], out var sid)) return;
			//Resolution First 42858 5.8s Second 41872 5.8s +2.5 each
			var dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance1";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[0] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.DestoryAt = 6000;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance2";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[1] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 6000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance3";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[2] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 7500;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance4";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[3] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 9000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance5";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[4] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 10500;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance6";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[5] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 12000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance7";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[6] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 13500;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

			dp = accessory.Data.GetDefaultDrawProperties();
			dp.Name = "Dance8";
			dp.Scale = new(80, 80);
			dp.Owner = sid;
			dp.Rotation = (Dance[7] - 1) * float.Pi / 2;
			dp.Color = accessory.Data.DefaultDangerColor;
			dp.Delay = 15000;
			dp.DestoryAt = 1500;
			accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
		}

		[ScriptMethod(name: "LittleFrog2_SafeZoneRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42867)$"], userControl: false)]
		public void LittleFrog2_SafeZoneRecord(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4) return;
	
			//0-unknown, 1-center safe guide north-south, 2-center safe guide east-west, 3-north-south safe, 4-east-west safe
			if (@event.SourcePosition == new Vector3(95f, 0f, 80f) || @event.SourcePosition == new Vector3(105f, 0f, 120f))
			{
				SituationInFrog2 = 1;
			}
			if (@event.SourcePosition == new Vector3(80f, 0f, 105f) || @event.SourcePosition == new Vector3(120f, 0f, 95f))
			{
				SituationInFrog2 = 2;
			}
			if (@event.SourcePosition == new Vector3(80f, 0f, 95f) || @event.SourcePosition == new Vector3(120f, 0f, 105f))
			{
				SituationInFrog2 = 3;
			}
			if (@event.SourcePosition == new Vector3(105f, 0f, 80f) || @event.SourcePosition == new Vector3(95f, 0f, 120f))
			{
				SituationInFrog2 = 4;
			}

			Frog2Round++;

		}

		[ScriptMethod(name: "LittleFrog2_Navigation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42871)$"])]
		public void LittleFrog2_Navigation(Event @event, ScriptAccessory accessory)
		{
			if (parse != 4) return;
			var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
			Vector3 centre = new(100f, 0f, 100f);
			//TH
			if (myindex == 0)
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationMT";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ?  new Vector3(95.5f,0f,94.5f): centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationMT";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(94.5f, 0f, 95.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationMT";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(95f, 0f, 89.5f) : new Vector3(100f, 0f, 89f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationMT";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(89.5f, 0f, 95f) : new Vector3(89f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == (GlobalStrat == StgEnum.CnServer ? 3: 1))
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationH2" : "LittleFrog2_NavigationST";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(104.5f, 0f, 105.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationH2" : "LittleFrog2_NavigationST";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(105.5f, 0f, 104.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationH2" : "LittleFrog2_NavigationST";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(105f, 0f, 110.5f) : new Vector3(100f, 0f, 111f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationH2" : "LittleFrog2_NavigationST";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(110.5f, 0f, 105f) : new Vector3(111f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == 2)
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationH1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(95.5f, 0f, 105.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationH1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(94.5f, 0f, 104.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationH1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(95f, 0f, 110.5f) : new Vector3(100f, 0f, 111f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationH1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(89.5f, 0f, 105f) : new Vector3(89f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == (GlobalStrat == StgEnum.CnServer ? 1: 3))
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationST" : "LittleFrog2_NavigationH2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(104.5f, 0f, 94.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationST" : "LittleFrog2_NavigationH2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(105.5f, 0f, 95.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationST" : "LittleFrog2_NavigationH2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(105f, 0f, 89.5f) : new Vector3(100f, 0f, 89f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = GlobalStrat == StgEnum.CnServer ? "LittleFrog2_NavigationST" : "LittleFrog2_NavigationH2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 0 ? new Vector3(110.5f, 0f, 95f) : new Vector3(111f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}

			//DPS
			if (myindex == 4)
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(95.5f, 0f, 105.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(94.5f, 0f, 104.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(95f, 0f, 110.5f) : new Vector3(100f, 0f, 111f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD1";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(89.5f, 0f, 105f) : new Vector3(89f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == 5)
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(104.5f, 0f, 105.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(105.5f, 0f, 104.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(105f, 0f, 110.5f) : new Vector3(100f, 0f, 111f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD2";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(110.5f, 0f, 105f) : new Vector3(111f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == 6)
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(95.5f, 0f, 94.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(94.5f, 0f, 95.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(95f, 0f, 89.5f) : new Vector3(100f, 0f, 89f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD3";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(89.5f, 0f, 95f) : new Vector3(89f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
			}
			if (myindex == 7)
			{
				if (SituationInFrog2 == 1)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(104.5f, 0f, 94.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 2)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(105.5f, 0f, 95.5f) : centre;
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 3)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(105f, 0f, 89.5f) : new Vector3(100f, 0f, 89f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
					accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
				}
				if (SituationInFrog2 == 4)
				{
					var dp = accessory.Data.GetDefaultDrawProperties();
					dp.Name = "LittleFrog2_NavigationD4";
					dp.Scale = new(2);
					dp.ScaleMode |= ScaleMode.YByDistance;
					dp.Owner = accessory.Data.Me;
					dp.TargetPosition = Frog2Round == 1 ? new Vector3(110.5f, 0f, 95f) : new Vector3(111f, 0f, 100f);
					dp.Color = accessory.Data.DefaultSafeColor;
					dp.DestoryAt = 9000;
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

		public static float GetStatusRemainingTime(ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
		{
			if (battleChara == null || !battleChara.IsValid()) return 0;
			unsafe
			{
				BattleChara* charaStruct = (BattleChara*)battleChara.Address;
				var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
				return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
			}
		}
		
		public static Vector3 FoldPointHorizon(Vector3 point, float centerX)
		{
			return point with { X = 2 * centerX - point.X };
		}
		
		public static Vector3 FoldPointVertical(Vector3 point, float centerZ)
		{
			return point with { Z = 2 * centerZ - point.Z };
		}
		
		#endregion

	}
}
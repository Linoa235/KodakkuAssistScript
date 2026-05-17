using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using System.Reflection.Metadata;
using System.Net;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.Intrinsics.Arm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FFXIVClientStructs;
using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Veever.Shadowbringers.theGrandCosmos;

[ScriptType(name: "LV.80 The Grand Cosmos", territorys: [884], guid: "b3a2febd-73ff-44a9-a897-22fa50c74ff3",
    version: "0.0.0.3", author: "Veever", note: noteStr)]

public class the_Grand_Cosmos
{
    const string noteStr =
    """
    v0.0.0.3:
    1. Now supports text banner/TTS toggle/DR TTS toggle (make sure you have correctly installed the `DailyRoutines` plugin before using DR TTS toggle) (do not enable both TTS toggles at the same time)
    2. Marker toggle and local toggle are in user settings, you can choose to turn them off or on (local enabled by default)
    3. Someday! Maybe! Will add new broom purple circle detection method
    4. Someday! Maybe! Will add new Boss2 seed detection method
    5. Boss3 Crystal Lamp marker not implemented, too big to miss
    6. Supports dungeon waypoint navigation, red lines indicate aggro range
    7. If you feel the waypoints are insufficient, please let me know on Discord (24 waypoints should be enough (x))
    8. Don't leave the instance immediately, there may be残留
    Duckmen.
    """;

    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    [UserSetting("Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Marker Toggle (ON = local only, OFF = party)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public int DarkShockTTSCount;
    public int magicBroomPosCount;
    public int Boss2SeedsNotifyCount;

    public bool Boss2WindisWest;

    private readonly object DarkShockTTSLock = new object();
    private readonly object magicBroomPosLock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        accessory.Method.MarkClear();
        DarkShockTTSCount = 0;
        magicBroomPosCount = 0;
        Boss2WindisWest = false;
        Boss2SeedsNotifyCount = 0;
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    #region Waypoints
    [ScriptMethod(name: "Waypoint Group 1", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001", "Instance:80030049"])]
    public async void Group1(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(1000);
        DebugMsg("In Group1", accessory);
        var toPosColor = new Vector4(1.0f, 1.0f, 0.0f, 4.0f);

        List<Vector3> List1 = new List<Vector3>
        {
            new Vector3(-13.92f, 0.00f, 326.36f),
            new Vector3(-74.22f, 0.00f, 320.13f),
            new Vector3(-105.53f, 0.05f, 300.47f),
            new Vector3(-107.94f, 0.00f, 284.64f),
            new Vector3(-101.72f, 0.00f, 274.01f),
            new Vector3(-58.48f, 0.00f, 280.89f),
            new Vector3(-51.44f, 0.00f, 292.14f),
            new Vector3(-23.44f, 0.00f, 285.24f),
            new Vector3(-0.30f, 0.00f, 237.67f),
        };

        FastDp(accessory, "1-1", List1[0], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-2", List1[1], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-3", List1[2], toPosColor);
        FastDp(accessory, "1-4", List1[3], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-5", List1[4], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-6", List1[5], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-7", List1[6], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-8", List1[7], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "1-9", List1[8], toPosColor);
    }

    [ScriptMethod(name: "Delete Group1 + Add Group2", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:11290"])]
    public async void delGroup1Add2(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        await Task.Delay(1000);
        var toPosColor = new Vector4(1.0f, 1.0f, 0.0f, 4.0f);

        List<Vector3> List2 = new List<Vector3>
        {
            new Vector3(13.34f, -7.00f, 115.41f),
            new Vector3(64.77f, -13.98f, 35.54f),
            new Vector3(-0.40f, -14.00f, -7.85f),
        };

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Line1";
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 5.0f);
        dp.Position = new Vector3(20, -7, 100);
        dp.TargetPosition = new Vector3(20, -7, 130);
        dp.Scale = new Vector2(6, 30f);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Line2";
        dp1.Color = new Vector4(1.0f, 0.0f, 0.0f, 5.0f);
        dp1.Position = new Vector3(92, -14, 23);
        dp1.TargetPosition = new Vector3(104, -14, 23);
        dp1.Scale = new Vector2(6, 15f);
        dp1.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp1);

        FastDp(accessory, "2-1", List2[0], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "2-2", List2[1], toPosColor);
        FastDp(accessory, "2-2", List2[2], toPosColor);
    }

    [ScriptMethod(name: "Delete Group2 + Add Group3", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:11268"])]
    public async void delGroup2Add3(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        await Task.Delay(1000);
        var toPosColor = new Vector4(1.0f, 1.0f, 0.0f, 4.0f);

        List<Vector3> List3 = new List<Vector3>
        {
            new Vector3(-0.14f, -4.00f, -125.28f),
            new Vector3(28.01f, -3.99f, -159.18f),
            new Vector3(-27.77f, -3.99f, -158.92f),
            new Vector3(9.72f, 8.00f, -198.45f),
            new Vector3(42.29f, 8.00f, -192.16f),
            new Vector3(47.17f, 8.00f, -182.42f),
            new Vector3(78.04f, 8.00f, -186.34f),
            new Vector3(78.41f, 8.00f, -207.17f),
            new Vector3(32.57f, 8.00f, -219.26f),
            new Vector3(30.73f, 8.05f, -231.61f),
            new Vector3(13.86f, 8.00f, -239.76f),
            new Vector3(0.66f, 8.00f, -288.16f),
        };
        FastDp(accessory, "3-1", List3[0], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-2", List3[1], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-3", List3[2], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-4", List3[3], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-5", List3[4], toPosColor);
        FastDp(accessory, "3-6", List3[5], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-7", List3[6], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-8", List3[7], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-9", List3[8], toPosColor);
        FastDp(accessory, "3-10", List3[9], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-11", List3[10], accessory.Data.DefaultSafeColor);
        FastDp(accessory, "3-12", List3[11], accessory.Data.DefaultSafeColor);
    }

    public void FastDp(ScriptAccessory accessory, string name, Vector3 position, Vector4 color)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color;
        dp.Position = position;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(2);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Delete Draw 0", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:1223", "SourcePosition:{\"X\":-0.02,\"Y\":7.98,\"Z\":-355.00}"])]
    public async void delDraw0(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Delete Draw 1", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:11283"])]
    public async void delDraw1(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    #endregion

    #region Trash Mobs
    [ScriptMethod(name: "Cloudcover", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18720"])]
    public void Cloudcover(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cloudcover";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Smite of Rage", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18721"])]
    public void SmiteofRage(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Smite of Rage";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(5f,4f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Iron Justice", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18719"])]
    public void IronJustice(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Iron Justice";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(11f);
        dp.Radian = float.Pi / 180 * 120;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Self-Destruct", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18725"])]
    public void SelfDestruct(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Self-Destruct";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(7);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Nepenthic Plunge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18728"])]
    public void NepenthicPlunge(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Nepenthic Plunge";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(10f);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Brewing Storm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18729"])]
    public void BrewingStorm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Brewing Storm";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(5f);
        dp.Radian = float.Pi / 180 * 60;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Ronkan Cure II - Stun Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18737"])]
    public async void RonkanCureII(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stun Halmos", duration: 4000, true);
        accessory.TTS("Stun Halmos", isTTS, isDRTTS);
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Bind1, LocalMark);
        await Task.Delay(5000);
        accessory.Method.MarkClear();
    }
    #endregion

    #region Boss1
    [ScriptMethod(name: "Boss1 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18281"])]
    public void Boss1Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 4000, true);
        accessory.TTS("Tankbuster incoming", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Boss1 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18851"])]
    public void Boss1AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 3000, true);
        accessory.TTS("AOE", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Boss1 Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18282"])]
    public void Boss1Stack(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Boss1 Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 5000, true);
        accessory.TTS($"Stack with {tname}", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Dark Well", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0060"])]
    public void DarkWell(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("Spread out", duration: 5000, true);
            accessory.TTS("Spread out", isTTS, isDRTTS);
        }
    }

    [ScriptMethod(name: "Dark Shock", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18287"])]
    public void DarkShock(Event @event, ScriptAccessory accessory)
    {
        if (DarkShockTTSCount == 10)
        {
            if (isText) accessory.Method.TextInfo("Avoid yellow circles", duration: 3000, true);
            accessory.TTS("Avoid yellow circles", isTTS, isDRTTS);
        }
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Shock {DarkShockTTSCount}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Tribulation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18852"])]
    public void Tribulation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Tribulation";
        dp.Color = new Vector4(148 / 255.0f, 0 / 255.0f, 211 / 255.0f, 1.0f);
        dp.Position = @event.EffectPosition();  
        dp.Scale = new Vector2(3);
        dp.DestoryAt = 8500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Tribulation 2";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Position = @event.EffectPosition();
        dp1.Scale = new Vector2(6.5f);
        dp1.Delay = 13000;
        dp1.DestoryAt = 9500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);

        Task.Delay(9000);
        lock (DarkShockTTSLock)
        {
            DebugMsg($"DarkShockTTSCount = {DarkShockTTSCount}", accessory);
            if (DarkShockTTSCount == 0)
            {
                if (isText) accessory.Method.TextInfo("Group up to guide yellow circles, then avoid", duration: 3000, true);
                accessory.TTS("Group up to guide yellow circles, then avoid", isTTS, isDRTTS);
            }
            DarkShockTTSCount++;
        }
    }

    [ScriptMethod(name: "Broom Position", eventType: EventTypeEnum.SetObjPos, eventCondition: ["Id:003E", "MorlogId:106"])]
    public async void magicBroomPos(Event @event, ScriptAccessory accessory)
    {
        lock (magicBroomPosLock)
        {
            DebugMsg($"magicBroomPosCount: {magicBroomPosCount}", accessory);
            if (magicBroomPosCount <= 4 || (magicBroomPosCount >= 10 && magicBroomPosCount <= 14 ))
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Broom Direction";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = @event.SourceId();
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Scale = new Vector2(1, 5.5f);
                dp.DestoryAt = 14500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"Broom Range Position";
                dp1.Color = new Vector4(255 / 255.0f, 215 / 255.0f, 0 / 255.0f, 1.0f);
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(3f);
                dp1.DestoryAt = 14500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
            }
            magicBroomPosCount++;
        }
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "Boss2 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18203"])]
    public void Boss2Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 4000, true);
        accessory.TTS("Tankbuster incoming", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Boss2 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18204"])]
    public void Boss2AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 3000, true);
        accessory.TTS("AOE", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Boss2 Seed Transport Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18206"])]
    public void Boss2SeedsNotify(Event @event, ScriptAccessory accessory)
    {
        if (Boss2SeedsNotifyCount == 0)
        {
            if (isText) accessory.Method.TextInfo("Move seeds away from the grass", duration: 8000, true);
            accessory.TTS("Move seeds away from the grass", isTTS, isDRTTS);
        } else
        {
            if (isText) accessory.Method.TextInfo("Seeds will be knocked back one step, move seeds to safe zone", duration: 8000, true);
            accessory.TTS("Seeds will be knocked back one step, move seeds to safe zone", isTTS, isDRTTS);
        }
    }

    [ScriptMethod(name: "Ode to Fallen Petals", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18768"])]
    public void OdetoFallenPetals(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Dynamo, go under the boss", duration: 4000, true);
        accessory.TTS("Dynamo, go under the boss", isTTS, isDRTTS);

        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = $"Ode to Fallen Petals (Single Dynamo)";
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(5);
        dp2.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);

        var dp3 = accessory.Data.GetDefaultDrawProperties();
        dp3.Name = $"Ode to Fallen Petals Waypoint";
        dp3.Owner = accessory.Data.Me;
        dp3.Color = accessory.Data.DefaultSafeColor;
        dp3.ScaleMode |= ScaleMode.YByDistance;
        dp3.TargetPosition = @event.SourcePosition();
        dp3.Scale = new(2);
        dp3.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
    }

    [ScriptMethod(name: "Ireful Wind Knockback Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18209"])]
    public async void IrefulWind(Event @event, ScriptAccessory accessory)
    {
        var westPos = new Vector3(-21.57f, -12.50f, -60.00f);

        if (@event.SourcePosition() == westPos)
        {
            Boss2WindisWest = true;
        }

        for (int i = 0; i <= 24; i++)
        {
            await Task.Delay(500);
            var offset = Boss2WindisWest ? -1f : 1f;
            foreach (var item in IbcHelper.GetByDataId(accessory, 11269))
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Ireful Wind Seed Knockback Reminder";
                dp.Scale = new(1.5f, 10);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = item.EntityId;

                var targetPosition = item.Position;
                targetPosition.X += offset;
                dp.TargetPosition = targetPosition;
                dp.Rotation = float.Pi + item.Rotation * float.Pi / 180;
                dp.DestoryAt = 500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "Left/Right Knout", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(1827[45])$"])]
    public void Boss3Knout(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId() == 18274;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"{(isR ? "Left" : "Right")} Knout";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5500;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi / 180 * 180;
        dp.Rotation = float.Pi / 180 * 90 * (isR ? -1 : 1);

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        if (isText) accessory.Method.TextInfo($"Go to the boss's {(isR ? "left" : "right")} side", duration: 4000, true);
        accessory.TTS($"Go to the boss's {(isR ? "left" : "right")} side", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Fire Marker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0019"])]
    public async void fireIcon(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo($"Spread, stay away from furniture, guide fire circle + cross", duration: 4000, true);
            accessory.TTS($"Spread, stay away from furniture, guide fire circle plus cross", isTTS, isDRTTS);
        }

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Fire Marker Circle";
        dp1.Color = new Vector4(255 / 255.0f, 127 / 255.0f, 80 / 255.0f, 1.0f);
        dp1.Owner = @event.TargetId();
        dp1.Scale = new Vector2(7);
        dp1.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }

    [ScriptMethod(name: "Otherworldly Heat", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18268"])]
    public void OtherworldlyHeat(Event @event, ScriptAccessory accessory)
    {
        var posCenter = @event.EffectPosition();
        var posLeftStart = posCenter;
        var posLeftEnd = posCenter;

        posLeftStart.X += 10.2f / 2;
        posLeftStart.X -= 10.2f / 2;

        for (var i = 0; i < 4; i++)
        {
            float rotation = 0;
            switch (i)
            {
                case 0: rotation = float.Pi / 180 * -90; break;
                case 1: rotation = float.Pi / 180 * 90; break;
                case 2: rotation = float.Pi; break;
                case 3: rotation = 0; break;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Otherworldly Heat left";
            dp.Color = new Vector4(128 / 255.0f, 0 / 255.0f, 128 / 255.0f, 1.0f);
            dp.Position = posLeftStart;
            dp.TargetPosition = posLeftEnd;
            dp.Rotation = rotation;
            dp.Scale = new Vector2(4f, 10.3f);
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }

    [ScriptMethod(name: "Blue Fire - Pass the Flame", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00C3"])]
    public async void BluefireIcon(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo($"Find furniture to pass the flame", duration: 4000, true);
            accessory.TTS($"Find furniture to pass the flame", isTTS, isDRTTS);
        }

        List<uint> dataIds = new List<uint> { 11278, 11279, 11281, 11280 };
        foreach (var dataId in dataIds)
        {
            foreach (var item in IbcHelper.GetByDataId(accessory, dataId))
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Blue Fire - Pass the Flame";
                dp.Color = new Vector4(0 / 255.0f, 191 / 255.0f, 255 / 255.0f, 1.0f);
                dp.Position = item.Position;
                dp.ScaleMode = ScaleMode.ByTime;
                dp.Scale = new Vector2(2);
                dp.DestoryAt = 15000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
    }

    [ScriptMethod(name: "Fire's Domain Marker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:003[2345]"])]
    public async void FiresDomainIcon(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo($"Spread away from boss, break the tether to make it purple, and don't guide the boss to furniture", duration: 7500, true);
            accessory.TTS($"Spread away from boss, break the tether to make it purple, and don't guide the boss to furniture", isTTS, isDRTTS);
        }

        DebugMsg($"Start Marking, Id: {@event.Id()}", accessory);
        if (@event.Id() == 26)
        {
            DebugMsg("Start 0032", accessory);
            if (isMark) accessory.Method.Mark(@event.TargetId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
        }
        if (@event.Id() == 27)
        {
            DebugMsg("Start 0033", accessory);
            if (isMark) accessory.Method.Mark(@event.TargetId(), KodakkuAssist.Module.GameOperate.MarkType.Attack2, LocalMark);
        }
        if (@event.Id() == 28)
        {
            DebugMsg("Start 0034", accessory);
            if (isMark) accessory.Method.Mark(@event.TargetId(), KodakkuAssist.Module.GameOperate.MarkType.Attack3, LocalMark);
        }
        if (@event.Id() == 29)
        {
            DebugMsg("Start 0035", accessory);
            if (isMark) accessory.Method.Mark(@event.TargetId(), KodakkuAssist.Module.GameOperate.MarkType.Attack4, LocalMark);
        }
        DebugMsg("End Marking", accessory);
    }

    [ScriptMethod(name: "Fire's Domain Tether", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0039"])]
    public async void FiresDomainTether(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fire's Domain Tether";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Fire's Ire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18273"])]
    public async void FiresIre(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fire's Ire";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode |= ScaleMode.ByTime;
        dp.Scale = new Vector2(20);
        dp.DestoryAt = 2000;
        dp.Radian = float.Pi / 180 * 90;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Boss3 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18277"])]
    public void Boss3AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 5700, true);
        accessory.TTS("AOE", isTTS, isDRTTS);
        accessory.Method.MarkClear();
    }

    [ScriptMethod(name: "Fall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18279"])]
    public async void fall(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fall";
        dp.Color = new Vector4(0 / 255.0f, 191 / 255.0f, 255 / 255.0f, 1.0f);
        dp.Position = @event.EffectPosition();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(3);
        dp.DestoryAt = 1600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Boss3 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:18276"])]
    public void Boss3Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 4000, true);
        accessory.TTS("Tankbuster incoming", isTTS, isDRTTS);
        accessory.Method.MarkClear();
    }
    #endregion
}

// EventExtensions, Extensions, IbcHelper classes (same as previous files)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FFXIVClientStructs;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Veever.DawnTrail.the_Underkeep;

[ScriptType(name: "LV.100 The Underkeep", territorys: [1266], guid: "025a8859-7d10-4e32-ad06-b879d50721ae",
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class the_Underkeep
{
    const string noteStr =
    """
    v0.0.0.4:
    1. Continuously updating
    2. Boss3 cross bomb drawing is incomplete, not yet available
    3. If you need a draw or notice any issues, @ me on DC or DM me.
    Duckmen.
    """;

    [UserSetting("Text banner toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;

    [UserSetting("Guide arrow toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Marker toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local marker toggle (ON = local only, OFF = party shared)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Debug switch, turn off unless developing")]
    public bool isDebug { get; set; } = false;

    public KodakkuAssist.Data.IGameObject? Boss { get; set; }

    public int StaticForceCount; 
    public int ConcurrentFieldCount;

    public Vector3 Boss1Center = new Vector3(-248.00f, -70.00f, 122.00f);

    private readonly object StaticForceLock = new object();
    private readonly object ConcurrentFieldLock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        StaticForceCount = 0;
        ConcurrentFieldCount = 0;
        SupplementaryInit(accessory);
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "debug", eventType: EventTypeEnum.Chat, eventCondition: ["Message:debug"])]
    public async void debug(Event @event, ScriptAccessory accessory)
    {
        //DebugMsg($"Me:{IbcHelper.GetMe().Name}", accessory);
        //DebugMsg($"job:{IbcHelper.GetMe().ClassJob.Value.Name}", accessory);
    }

    #region Adds
    [ScriptMethod(name: "Sandstorm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42904"])]
    public void Sandstorm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Sandstorm-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Sandstorm Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42904"], userControl: false)]
    public void SandstormClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Sandstorm-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Ultravibration", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42907"])]
    public void Ultravibration(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ultravibration-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Ultravibration Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42907"], userControl: false)]
    public void UltravibrationClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Ultravibration-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Sand Crusher", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42905"])]
    public void SandCrusher(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SandCrusher-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Sand Crusher Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42905"], userControl: false)]
    public void SandCrusherClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"SandCrusher-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42911"])]
    public void Earthquake(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Earthquake-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Earthquake Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42911"], userControl: false)]
    public void EarthquakeClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Earthquake-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Piercing Joust", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42917"])]
    public void PiercingJoust(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PiercingJoust-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Piercing Joust Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42917"], userControl: false)]
    public void PiercingJoustClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"PiercingJoust-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Blazing Torch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42921"])]
    public void BlazingTorch(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"BlazingTorch-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10);
        dp.Radian = float.Pi / 180 * 120;
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Blazing Torch Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42921"], userControl: false)]
    public void BlazingTorchClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"BlazingTorch-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Run Amok", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42924"])]
    public void RunAmok(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Run Amok-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6, 18);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Run Amok Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42924"], userControl: false)]
    public void RunAmokClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Run Amok-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Wheeling Shot", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42910"])]
    public void WheelingShot(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Go behind {@event.TargetName()} or interrupt", duration: 3500, true);
        if (isTTS) accessory.Method.EdgeTTS($"Go behind {@event.TargetName()} or interrupt");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Wheeling Shot-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi;
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Wheeling Shot Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42910"], userControl: false)]
    public void WheelingShotClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Wheeling Shot-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Electrostrike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42926"])]
    public void Electrostrike(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Electrostrike-{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Electrostrike Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:42926"], userControl: false)]
    public void ElectrostrikeClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Electrostrike-{@event.SourceId()}");
    }

    #endregion

    #region Boss 1
    [ScriptMethod(name: "Boss1 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42547|42544)$"])]
    public void Boss1AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4700, true);
        if (isTTS)  accessory.Method.EdgeTTS($"AOE");
    }

    [ScriptMethod(name: "Almighty Racket", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42546"])]
    public void AlmightyRacket(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Go behind boss", duration: 3500, true);
        if (isTTS) accessory.Method.EdgeTTS($"Go behind boss");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Almighty Racket";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = float.Pi;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Aerial Ambush", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42543"])]
    public void AerialAmbush(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away from line impact", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS($"Move away from line impact");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Aerial Ambush";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.TargetPosition = Boss1Center;
        dp.Scale = new Vector2(15, 30);
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Sedimentary Debris", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43160"])]
    public void SedimentaryDebris(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("Spread, do not overlap", duration: 4000, true);
            if (isTTS) accessory.Method.EdgeTTS($"Spread, do not overlap");
        }
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sedimentary Debris";
        dp.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 251 / 255.0f, 1.0f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 4800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Boss1 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42548"])]
    public void Boss1Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Prepare for tankbuster", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS($"Prepare for tankbuster");
    }

    [ScriptMethod(name: "Sphere Shatter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43135|42545)"])]
    public void SphereShatter(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sphere Shatter";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion

    #region Boss 2
    [ScriptMethod(name: "Boss2 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42579)$"])]
    public void Boss2AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4500, true);
        if (isTTS) accessory.Method.EdgeTTS($"AOE");
    }

    [ScriptMethod(name: "Boss2 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43136"])]
    public void Boss2Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Prepare for tankbuster", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS($"Prepare for tankbuster");
    }

    public bool isL = true;

    [ScriptMethod(name: "Sector Bisector", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4256[23])$"])]
    public void SectorBisector(Event @event, ScriptAccessory accessory)
    {
        // 42562 left
        // 42563 right
        if (@event.ActionId() != 42562) 
        { 
            isL = false; 
        } else {
            isL = true;
        }

        if (isText) accessory.Method.TextInfo($"Go to {(isL ? "right side of boss's last clone" : "left side of boss's last clone")}", duration: 3500, true);
        if (isTTS) accessory.Method.EdgeTTS($"Go to {(isL ? "right side of boss's last clone" : "left side of boss's last clone")}");
    }

    [ScriptMethod(name: "Ordered Fire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42573"])]
    public void OrderedFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ordered Fire";
        dp.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 251 / 255.0f, 1.0f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8, 55);
        if (@event.EffectPosition().Z < -200) 
        {
            dp.Rotation = 0;
        } else {
            dp.Rotation = float.Pi / 2;
        }
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Static Force", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:024F"])]
    public async void StaticForce(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(1000);
        lock (StaticForceLock)
        {
            if (StaticForceCount == 0)
            {
                DebugMsg($"{StaticForceCount}", accessory);
                if (isText) accessory.Method.TextInfo("Spread, do not overlap", duration: 4000, true);
                if (isTTS) accessory.Method.EdgeTTS($"Spread, do not overlap");
                for (var i = 0; i < accessory.Data.PartyList.Count; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "Static Force";
                    dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                    dp.Owner = @event.TargetId();
                    dp.TargetObject = accessory.Data.PartyList[i];
                    dp.Scale = new Vector2(40);
                    dp.Radian = float.Pi / 180 * 30;
                    dp.DestoryAt = 4800;
                    accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Fan, dp);
                }
            }
            StaticForceCount++;
            if (StaticForceCount > 3)
            {
                StaticForceCount = 0;
            }
            DebugMsg($"StaticForceCount: {StaticForceCount}", accessory);
        } 
    }

    [ScriptMethod(name: "Electric Excess", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43139"])]
    public void ElectricExcess(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("Spread, do not overlap", duration: 4000, true);
            if (isTTS) accessory.Method.EdgeTTS($"Spread, do not overlap");
        }
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Electric Excess";
        dp.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 251 / 255.0f, 1.0f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 4800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion

    #region Boss 3
    [ScriptMethod(name: "Boss3 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42525)$"])]
    public void Boss3AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4700, true);
        if (isTTS) accessory.Method.EdgeTTS($"AOE");
    }

    [ScriptMethod(name: "Electray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43130"])]
    public void Electray(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Electray";
        dp.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 251 / 255.0f, 1.0f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(9, 40);
        if (@event.EffectPosition().X > 13)
        {
            dp.Rotation = -float.Pi / 2;
        }
        else
        {
            dp.Rotation = float.Pi / 2;
        }
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Hypercharged Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42524"])]
    public void HyperchargedLight(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("Spread, do not overlap", duration: 4000, true);
            if (isTTS) accessory.Method.EdgeTTS($"Spread, do not overlap");
        }
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Hypercharged Light";
        dp.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 251 / 255.0f, 1.0f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 4800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Neutralize Front Lines", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42738"])]
    public void NeutralizeFrontLines(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Neutralize Front Lines";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = float.Pi;
        dp.DestoryAt = 4800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Deterrent Pulse", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42540"])]
    public async void DeterrentPulse(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown target";

        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 4700, true);
        accessory.Method.EdgeTTS($"Stack with {tname}");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Deterrent Pulse";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(8, 40);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Concurrent Field", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:024A"])]
    public async void ConcurrentField(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(1000);
        lock (ConcurrentFieldLock)
        {
            if (ConcurrentFieldCount == 0)
            {
                DebugMsg($"{ConcurrentFieldCount}", accessory);
                if (isText) accessory.Method.TextInfo("Spread, do not overlap", duration: 4000, true);
                if (isTTS) accessory.Method.EdgeTTS($"Spread, do not overlap");
                for (var i = 0; i < accessory.Data.PartyList.Count; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "Concurrent Field";
                    dp.Color = new Vector4(0.0f, 0.749f, 1.0f, 1.0f);
                    dp.Owner = @event.TargetId();
                    dp.TargetObject = accessory.Data.PartyList[i];
                    dp.Scale = new Vector2(40);
                    dp.Radian = float.Pi / 180 * 50;
                    dp.DestoryAt = 4800;
                    accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Fan, dp);
                }
            }
            ConcurrentFieldCount++;
            if (ConcurrentFieldCount > 3) 
            {
                ConcurrentFieldCount = 0;
            }
            DebugMsg($"StaticForceCount: {ConcurrentFieldCount}", accessory);
        }
    }
    #endregion

    #region Supplementary

    private Dictionary<uint, uint> _boss2ClonesDict = new();

    private void SupplementaryInit(ScriptAccessory sa)
    {
        _boss2ClonesDict.Clear();
    }

    [ScriptMethod(name: "Supplementary-Boss2 Clone Half-room Cleave Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0147"], userControl: false)]
    public void Boss2ClonesTetherRecord(Event evt, ScriptAccessory sa)
    {
        _boss2ClonesDict[evt.SourceId()] = evt.TargetId();
    }

    [ScriptMethod(name: "Supplementary-Boss2 Clone Half-room Cleave", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4316[34])$"], suppress: 8000)]
    public void Boss2ClonesCleave(Event evt, ScriptAccessory sa)
    {
        if (!_boss2ClonesDict.ContainsKey(evt.SourceId()))
        {
            _boss2ClonesDict.Clear();
            return;
        }
        var lastClone = _boss2ClonesDict[evt.SourceId()];
        var clonesNum = _boss2ClonesDict.Count;
        _boss2ClonesDict.Clear();

        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "Boss2ClonesCleave";
        dp.Color = sa.Data.DefaultDangerColor;
        dp.Scale = new Vector2(45);
        dp.Radian = float.Pi;
        dp.DestoryAt = (clonesNum - 1) * 900 + 500;
        dp.Owner = lastClone;
        dp.Rotation = evt.ActionId > 43163 ? -float.Pi / 2 : float.Pi / 2;

        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    #endregion
}

[Le code des classes d'extension (EventExtensions, IbcHelper, etc.) reste identique Ã  l'original car dÃ©jÃ  en anglais]
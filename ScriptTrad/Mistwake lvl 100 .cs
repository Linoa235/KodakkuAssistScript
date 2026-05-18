using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using KodaMarkType = KodakkuAssist.Module.GameOperate.MarkType;

namespace Veever.DawnTrail.Mistwake;

[ScriptType(name: Name, territorys: [1314], guid: "da4f5921-9426-4894-9c30-2c59bc2e307b",
    version: Version, author: "Linoa235", note: NoteStr, updateInfo: UpdateStr)]

public class Mistwake
{
    const string NoteStr =
    """
    v0.0.0.1
    ----- Please read the notes before use and adjust user settings as needed. -----
    ----- Please support Tetora! Meow! Thank you! Meow! -----
    1. If you need a draw or notice any issues, @ me on DC or DM me.
    2. The safe zone behind the rock during Boss1 not be perfectly precise. Please adjust your position as needed during the duty.
    Duckmen.
    """;

    const string UpdateStr =
    """
    v0.0.0.1
    Duckmen.
    """;

    private const string Name = "LV.100 Mistwake";
    private const string Version = "0.0.0.1";
    private const string DebugVersion = "a";

    private const bool Debugging = false;


    [UserSetting("Language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Draw opacity â€” higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("EdgeTTS toggle")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Auto anti-knockback")]
    public bool useAntiKnockBack { get; set; } = false;

    [UserSetting("Guide arrow toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Debug on/off (don't touch unless you know what you're doing)")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }


    private readonly object CountLock = new object();


    public void DebugMsg(string str, ScriptAccessory sa)
    {
        if (!isDebug) return;
        sa.Log.Debug($"[DEBUG] {str}");
    }

    private ScriptAccessory _sa = null;
    
    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        
        _sa = sa;
        
        _ = ScriptVersionChecker.CheckVersionAsync(
            sa,
            "b7d5e223-17b8-43bf-932f-dceddf10ba1a",
            Version,
            showNotification: true
        );

        
        sa.Method.ClearFrameworkUpdateAction(this);

        RefreshParams();
    }
     
    private void RefreshParams()
    {
        _Boss3FulgurousFallGuid = "";
        _Boss3Pos = -1;
        _Boss3FulgurousFallCheck = -1;
    }
    
    #region Mobs
    [ScriptMethod(name: "---- Mobs ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void mobs(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Head Injury & Interject Interrupt Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void destroyCancelAction(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($".*{ev.TargetId}");
    }
    
    [ScriptMethod(name: "Static Storm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45204"])]
    public void StaticStorm(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(8f), 120, 4000, $"StaticStorm-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Static Storm Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:45204"], userControl: false)]
    public void StaticStormClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"StaticStorm-{ev.SourceId}");
    }
    
    [ScriptMethod(name: "Lightning Bolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46180"])]
    public void LightningBolt(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Lightning Bolt-{ev.SourceId}", color: sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Lightning Bolt Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:46180"], userControl: false)]
    public void LightningBoltClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Lightning Bolt-{ev.SourceId}");
    }

    [ScriptMethod(name: "Thunderbolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45206"])]
    public void Thunderbolt(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(12f), 120, 3700, $"Thunderbolt-{ev.SourceId}", color: sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Thunderbolt Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:45206"], userControl: false)]
    public void ThunderboltClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Thunderbolt-{ev.SourceId}");
    }
    
    [ScriptMethod(name: "Knowing Gleam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45207"])]
    public void KnowingGleam(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 3700, $"Knowing Gleam-{ev.SourceId}", color: sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Knowing Gleam Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:45207"], userControl: false)]
    public void KnowingGleamClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Knowing Gleam-{ev.SourceId}");
    }
    
    [ScriptMethod(name: "Megablaster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45209"])]
    public void Megablaster(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(10f), 90, 3700, $"Megablaster-{ev.SourceId}", color: sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Megablaster Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:45209"], userControl: false)]
    public void MegablasterClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Megablaster-{ev.SourceId}");
    }

    [ScriptMethod(name: "Thunderstrike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45208"])]
    public void Thunderstrike(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 3700, $"Thunderstrike-{ev.SourceId}", color: sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Thunderstrike Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:45208"], userControl: false)]
    public void ThunderstrikeClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Thunderstrike-{ev.SourceId}");
    }
    #endregion


    #region Boss1
    [ScriptMethod(name: "---- Boss1 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void Boss1(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43327"])]
    public void Boss1MedicineField(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        sa.TTS($"{msg}", isEdgeTTS);
    }
    
    [ScriptMethod(name: "Thunder III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43329"])]
    public void Boss1ThunderIII(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "AOE tankbuster â€” Stay away from party" : "AOE tankbuster â€” Stay away from party";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
        }
        else
        {
            string msg = language == Language.Chinese ? "Avoid AOE tankbuster" : "Avoid AOE tankbuster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
        }
    }
    
    [ScriptMethod(name: "Bedeviling Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43330"])]
    public void BedevilingLight(Event ev, ScriptAccessory sa)
    {
        uint Boss1DataID = 18497;
        List<IGameObject> BedevilingLightObjects = new();
        List<uint> DataList = new List<uint> { 18498, 18513, 18499 };
        
        foreach (var obj in sa.Data.Objects)
        {
            if (DataList.Contains(obj.DataId))
            {
                BedevilingLightObjects.Add(obj);
                if (isDebug) sa.Log.Debug($"Dataid: {obj.DataId},pos: {obj.Position}");
            }
        }

        var BossObj = IbcHelper.GetByDataId(sa, Boss1DataID).FirstOrDefault();
        foreach (var obj in BedevilingLightObjects)
        {
            DrawHelper.DrawFanPos(sa, obj.Position, BossObj.Position, float.Pi, new Vector2(15f), 35f,
                7000, $"{obj.EntityId} - BedevilingLight", color: sa.Data.DefaultSafeColor, scaleByTime: false);
        }
        
        string msg = language == Language.Chinese ? "Hide behind the rock" : "Hide behind the rock";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
    }

    [ScriptMethod(name: "Ray of Lightning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44825"])]
    public void Boss1RayofLightning(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        string msg = language == Language.Chinese ? $"Stack with {tname}" : $"Stack with {tname}";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);

        DrawHelper.DrawRectObjectTarget(sa, ev.SourceId, ev.TargetId, new Vector2(5f, 50f), 
            6200, $"Ray of Lightning - {ev.SourceId} - {ev.TargetId}", color: sa.Data.DefaultSafeColor);
    }
    
    [ScriptMethod(name: "Petribreath", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43335"])]
    public void Petribreath(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObjectNoRot(sa, ev.SourceId, new Vector2(30f), 120, 4700, $"Petribreath", color: sa.Data.DefaultDangerColor);
    }
    
    [ScriptMethod(name: "Thunder II", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43331"])]
    public void Boss1ThunderII(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Spread and Avoid the Rocks" : $"Spread and Avoid the Rocks";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 2800, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
    }
    #endregion
        
    
    #region Boss2
    [ScriptMethod(name: "---- Boss2 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void Boss2(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Thunderclap Concerto", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45337|45342)$"])]
    public void ThunderclapConcerto(Event ev, ScriptAccessory sa)
    {
        var rotDanger = ev.ActionId == 45337 ? 0f.DegToRad() : 180f.DegToRad();
        var rotSafe = ev.ActionId == 45342 ? 0f.DegToRad() : 180f.DegToRad();
        
        DrawHelper.DrawFanObject(sa, ev.SourceId, rotDanger, new Vector2(22f), 300, 5200,
            $"Thunderclap Concerto - {ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: false);
        
        DrawHelper.DrawFanObject(sa, ev.SourceId, rotSafe, new Vector2(22f), 60, 5200,
            $"Thunderclap Concerto - {ev.SourceId}", color: sa.Data.DefaultSafeColor, scaleByTime: false);
    }
    
    [ScriptMethod(name: "Bio II", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45345"])]
    public void BioII(Event ev, ScriptAccessory sa)
    {
        if (isText) sa.Method.TextInfo($"AOE", duration: 4300, true);
        if (isTTS) sa.TTS("AOE", isEdgeTTS);
    }
    
    private readonly Queue<string> _thunderChargeDraws = new();
    private int _drawCounter = 0;
    
    [ScriptMethod(name: "Galloping Thunder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45348"])]
    public void GallopingThunder(Event ev, ScriptAccessory sa)
    {
        var drawName = $"Galloping Thunder - {ev.SourceId}_{++_drawCounter}";
        _thunderChargeDraws.Enqueue(drawName);
        
        DrawHelper.DrawRectObjectTargetPos(sa, ev.SourceId, ev.EffectPosition, new Vector2(5), 15000, drawName, new Vector4(1, 0, 0, ColorAlpha), scalemode: ScaleMode.YByDistance);
    }

    [ScriptMethod(name: "Galloping Thunder Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:45347"], userControl: false)]
    public void GallopingThunderDestroy(Event ev, ScriptAccessory sa)
    {   
        if (_thunderChargeDraws.Count > 0)
        {
            sa.Method.RemoveDraw(_thunderChargeDraws.Dequeue());
        }
    }
    
    [ScriptMethod(name: "Burst", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2536"])]
    public void Burst(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(9f), 4500, $"Burst - {ev.TargetId}", color: sa.Data.DefaultDangerColor);
    }
    
    [ScriptMethod(name: "Burst Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:45349"],userControl: false)]
    public void BurstDestroy(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Burst - {ev.TargetId}");
    }
    
    [ScriptMethod(name: "Thunder IV", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45351"])]
    public void ThunderIV(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE, Stay Away from Thunder Ball" : "AOE, Stay Away from Thunder Ball";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4300, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
        
        foreach (var item in sa.Data.Objects.GetByDataId(19064))
        {
            if (item is IBattleChara chara)
            {
                if (!IbcHelper.HasStatus(sa, chara, 0x9E8))
                {
                    DrawHelper.DrawCircleObject(sa, item.EntityId, new Vector2(9f), 5700, "Thunder IV", sa.Data.DefaultDangerColor, scaleByTime: false);
                }
            }
        }
    }
    
    [ScriptMethod(name: "Thunder III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45353"])]
    public void ThunderIII(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6f), 7200, "Thunder III",  sa.Data.DefaultSafeColor, scaleByTime: false);
    }           
    
    [ScriptMethod(name: "Shockbolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45356"])]
    public void Shockbolt(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Tank Buster" : $"Tank Buster";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4300, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "---- Boss3 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void Boss3(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Thunderspark", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45291"])]
    public void Thunderspark(Event ev, ScriptAccessory sa)
    {
        string msg = $"AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4300, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
        
        sa.Method.ClearFrameworkUpdateAction(this);
        _Boss3FulgurousFallGuid = "";
        _Boss3Pos = -1;
        _Boss3FulgurousFallCheck = -1;
    }
    
    [ScriptMethod(name: "Golden Talons", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45305"])]
    public void GoldenTalons(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Tank Buster" : $"Tank Buster";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4300, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
    }
    
    [ScriptMethod(name: "Thunderbolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4529[678]|4694[34])$"])]
    public void Boss3Thunderbolt(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "Thunderbolt";
        dp.Owner = ev.SourceId;
        dp.Color = sa.Data.DefaultDangerColor.WithW(0.6f);
        dp.Scale = new (6f, 92f);
        dp.DestoryAt = 5200;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    private string _Boss3FulgurousFallGuid = "";
    private float _Boss3FulgurousFallCheck = -1;
    private int _Boss3Pos = -1;

    private void Boss3FulgurousFallFrameworkAction()
    {
        var sa = _sa;
        var myObj = sa.Data.MyObject;
        if (myObj is null) return;
        var myPos = myObj.Position;
        
        if (_Boss3Pos == -1) return;

        if (_Boss3Pos == 1)
        {
            sa.Log.Debug("in X");
            if (myPos.Z < -620)
            {
                sa.Log.Debug("in  < -620");
                if (_Boss3FulgurousFallCheck == 0) return;
                sa.Method.RemoveDraw($"Fulgurous Fall Displacement Line 1");
                var dp = sa.Data.GetDefaultDrawProperties();
                dp.Name = $"Fulgurous Fall Displacement Line 0";
                dp.Scale = new(1.5f, 12);
                dp.Color = sa.Data.DefaultSafeColor;
                dp.Owner = sa.Data.Me;
                dp.Rotation = float.Pi;
                dp.FixRotation = true;
                dp.DestoryAt = 5700;
                if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                _Boss3FulgurousFallCheck = 0;
            }
            else if (myPos.Z > -620)
            {
                sa.Log.Debug("in myPosZ > -620");
                if (_Boss3FulgurousFallCheck == 1) return;
                sa.Method.RemoveDraw($"Fulgurous Fall Displacement Line 0");
                var dp = sa.Data.GetDefaultDrawProperties();
                dp.Name = $"Fulgurous Fall Displacement Line 1";
                dp.Scale = new(1.5f, 12);
                dp.Color = sa.Data.DefaultSafeColor;
                dp.Owner = sa.Data.Me;
                dp.Rotation = 0;
                dp.FixRotation = true;
                dp.DestoryAt = 5700;
                if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                _Boss3FulgurousFallCheck = 1;
            } 
        } else if (_Boss3Pos == 2)
        {
            if (myPos.X < 281)
            {
                sa.Log.Debug("in .X < 281");
                if (_Boss3FulgurousFallCheck == 0) return;
                sa.Method.RemoveDraw($"Fulgurous Fall Displacement Line 1");
                var dp = sa.Data.GetDefaultDrawProperties();
                dp.Name = $"Fulgurous Fall Displacement Line 0";
                dp.Scale = new(1.5f, 12);
                dp.Color = sa.Data.DefaultSafeColor;
                dp.Owner = sa.Data.Me;
                dp.Rotation = -float.Pi / 2;
                dp.FixRotation = true;
                dp.DestoryAt = 5700;
                if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                _Boss3FulgurousFallCheck = 0;
            }
            else if (myPos.X > 281)
            {
                sa.Log.Debug("in .X > 281");
                if (_Boss3FulgurousFallCheck == 1) return;
                sa.Method.RemoveDraw($"Fulgurous Fall Displacement Line 0");
                var dp = sa.Data.GetDefaultDrawProperties();
                dp.Name = $"Fulgurous Fall Displacement Line 1";
                dp.Scale = new(1.5f, 12);
                dp.Color = sa.Data.DefaultSafeColor;
                dp.Owner = sa.Data.Me;
                dp.Rotation = float.Pi / 2;
                dp.FixRotation = true;
                dp.DestoryAt = 5700;
                if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                _Boss3FulgurousFallCheck = 1;
            } 
        }

    }
    
    [ScriptMethod(name: "Fulgurous Fall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45301"])]
    public async void FulgurousFall(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Knockback from the center, then dodge line AOEs" : $"Knockback from the center, then dodge line AOEs";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 5300, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
        
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 50f), 5700, "Straight Danger", sa.Data.DefaultDangerColor);
        
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(3f, 50f),
            5700, "Straight - Safe1", sa.Data.DefaultSafeColor, offset: new Vector3(6.5f, 0, 0));
        
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(3f, 50f),
            5700, "Straight - Safe2", sa.Data.DefaultSafeColor, offset: new Vector3(-6.5f, 0, 0));

        var sPos = ev.SourcePosition;
        if ((sPos.X < 264 && sPos.Z < -617) || (sPos.X > 297 && sPos.Z < -617))
        {
            _Boss3Pos = 1;
        }
        else if ((sPos.X < 285 && sPos.Z > -603) || (sPos.X < 285 && sPos.Z < -635))
        {
            _Boss3Pos = 2;
        }

        _Boss3FulgurousFallGuid = sa.Method.RegistFrameworkUpdateAction(Boss3FulgurousFallFrameworkAction);

        await Task.Delay(1500);
        if (useAntiKnockBack) sa.Method.UseAction(sa.Data.Me, 7559);
        if (useAntiKnockBack) sa.Method.UseAction(sa.Data.Me, 7548);
    }
    
    
    [ScriptMethod(name: "Electrogenetic Force", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:45302"])]
    public void ElectrogeneticForce(Event ev, ScriptAccessory sa)
    {
        sa.Method.UnregistFrameworkUpdateAction(_Boss3FulgurousFallGuid);
        _Boss3FulgurousFallGuid = "";
        _Boss3Pos = -1;
        _Boss3FulgurousFallCheck = -1;
        sa.Method.RemoveDraw("Fulgurous Fall Displacement Line.*");
        
        string msg = language == Language.Chinese ? $"Move out!" : $"Move out!";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
        if (isTTS) sa.TTS($"{msg}", isEdgeTTS);
        
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "Electrogenetic Force";
        dp.Owner = ev.SourceId;
        dp.Color = new Vector4(1f, 0f, 0f, ColorAlpha);
        dp.Scale = new (40f, 18f);
        dp.DestoryAt = 4500;
        dp.ScaleMode = ScaleMode.ByTime;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    #endregion
}
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace PVPAction;

[ScriptType(guid: "777d5222-2993-4d12-92ec-acaa586c7617", name: "PVP Skill Drawing", territorys: [],
    version: "0.0.0.8", author: "Linoa235", note: noteStr)]

public class PVPAction
{
    const string noteStr =
        """
        v0.0.0.8:
        PVP skill drawing, available on all maps, no regional restrictions.
        Recommended to go through settings first and disable unnecessary features.
        Remember to save after changing user settings!
        [Only apply to enemy target markers] is for frontlines, usually marked with x on targets.
        [Instant skill range prediction] requires manual macro trigger.
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("EdgeTTS Toggle")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Notification Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Enable only for enemy target markers [Applicable: Dragoon Stardiver drawing]")]
    public bool isOnlyMark { get; set; } = false;
    
    [UserSetting("Self skill colors (e.g., Holy Sheltron)")]
    public ScriptColor SelfAOEColor { get; set; } = new() { V4 = new(1f, 0f, 1f, 1f) };
    
    [UserSetting("Self skill fill brightness (recommended less than 1)")]
    public float SelfAOEFillBrightness { get; set; } = 0.4f;
    
    [UserSetting("Self control skill prediction color (e.g., Closed Position, Arcane Circle)")]
    public ScriptColor SelfCompulsoryControlColor { get; set; } = new() { V4 = new(1f, 0f, 0f, 10f) };
    
    [UserSetting("Self control skill prediction fill brightness (recommended less than 1)")]
    public float SelfCompulsoryControlFillBrightness { get; set; } = 0f;
    
    [UserSetting("Self control skill prediction display time (ms)")]
    public int MoveActionsTime { get; set; } = 10000;
    
    [UserSetting("Enemy skill colors (e.g., Astrologian, Bard LB)")]
    public ScriptColor EnmityAOEColor { get; set; } = new() { V4 = new(1f, 0f, 1f, 1f) };
    
    [UserSetting("Enemy skill fill brightness (recommended less than 1)")]
    public float EnmityAOEFillBrightness { get; set; } = 0.25f;
    
    [UserSetting("Enemy skill display time (ms)")]
    public long EnmityAOETimer { get; set; } = 3000;
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;
    
    private List<MarkType> checkMark = new List<MarkType>()
    {
        MarkType.Attack1,
        MarkType.Attack2,
        MarkType.Attack3,
        MarkType.Attack4,
        MarkType.Attack5,
        MarkType.Bind1,
        MarkType.Bind2,
        MarkType.Bind3,
        MarkType.Ignore1,
        MarkType.Ignore2,
        MarkType.Attack6,
        MarkType.Attack7,
        MarkType.Attack8,
    };
    
    #endregion
    
    public bool isPartyMember(ScriptAccessory accessory, uint SourceId)
    {
        return accessory.Data.PartyList.Contains(SourceId);
    }
    
    private bool PartyFilter(ScriptAccessory sa, IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return false;
        
        if (obj.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Pc) return false;

        var targetPlayer = obj as IPlayerCharacter;
        if (targetPlayer == null) return false;
        
        bool isInMyAlliance = false;
        
        isInMyAlliance = sa.Data.PartyList?.Any(p => p == targetPlayer.EntityId) ?? false;
        
        if (!isInMyAlliance)
        {
            if (targetPlayer.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.AllianceMember))
            {
                isInMyAlliance = true;
            }
        }

        return isInMyAlliance;
    }
    
    #region Role Actions
    
    [ScriptMethod(name: "Enemy Comet Judgment Time Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43252"])]
    public void CometEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;
        
        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Comet{@event.SourceId()}";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp.Position = @event.EffectPosition();
            dp.Scale = new Vector2(10f);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.DestoryAt = 2400;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Enemy Comet Destroy", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43252"],userControl: false)]
    public void EnemyCometDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Enemy Comet{@event.SourceId()}");
    }
    
    #endregion
    
    #region Dragoon
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Dragoon â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Dragoon(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Self Stardiver Range Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3180"])]
    public void SkyShatterSelf(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stardiver Self Inner";
        dp.Color = new Vector4(0f, 1f, 1f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Stardiver Self Outer";
        dp1.Color = new Vector4(0f, 1f, 1f, 0.5f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(10f);
        dp1.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }
    
    [ScriptMethod(name: "Self Stardiver Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3180"],userControl: false)]
    public void SelfStardiverDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        accessory.Method.RemoveDraw($"StardiverSelf.*");
    }
    

    [ScriptMethod(name: "Alliance Stardiver Range Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3180"])]
    public void SkyShatterAlliance(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;
        
        if (PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Alliance Stardiver{@event.SourceId()}";
            dp.Color = new Vector4(0f, 1f, 1f, 0.15f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(10f);
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
        
    [ScriptMethod(name: "Alliance Stardiver Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3180"],userControl: false)]
    public void AllianceStardiverDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Alliance Stardiver{@event.SourceId()}");
    }

    
    [ScriptMethod(name: "Enemy Stardiver Range Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3180"])]
    public void SkyShatterEnmity(Event @event, ScriptAccessory accessory)
    {
        if (isOnlyMark) return;
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Stardiver Inner{@event.SourceId()}";
            dp.Color = new Vector4(1f, 0f, 0f, 1f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Enemy Stardiver Outer{@event.SourceId()}";
            dp1.Color = new Vector4(1, 0f, 0f, 0.5f);
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(10f);
            dp1.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
        }
    }
    
    [ScriptMethod(name: "Enemy Marked Stardiver Range Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3180"])]
    public void SkyShatterMark(Event @event, ScriptAccessory accessory)
    {
        if (!isOnlyMark) return;
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var tid = @event.TargetId();
            var tobj = IbcHelper.GetById(accessory, tid);
            if (tobj == null || !tobj.IsValid()) return;

            if (!IbcHelper.HasAnyMarker(tobj)) return;

            foreach (var mark in checkMark)
            {
                if (IbcHelper.HasMarker(tobj, mark))
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Enemy Marked Stardiver Inner{@event.SourceId()}";
                    dp.Color = new Vector4(1f, 0f, 0f, 1f);
                    dp.Owner = @event.SourceId();
                    dp.Scale = new Vector2(5f);
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                    var dp1 = accessory.Data.GetDefaultDrawProperties();
                    dp1.Name = $"Enemy Marked Stardiver Outer{@event.SourceId()}";
                    dp1.Color = new Vector4(1, 0f, 0f, 0.6f);
                    dp1.Owner = @event.SourceId();
                    dp1.Scale = new Vector2(10f);
                    dp1.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                }
            }
        }
    }
        
    [ScriptMethod(name: "Enemy Stardiver Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3180"],userControl: false)]
    public void EnemyStardiverDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Enemy.*Stardiver.*{@event.SourceId()}");
    }
    
    #endregion

    #region Sage
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Sage â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Sage(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Party Mesotes Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29266"])]
    public void MesotesParty(Event @event, ScriptAccessory accessory)
    {
        if (isPartyMember(accessory, @event.SourceId()))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Party Mesotes{@event.SourceId()}";
            dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
            dp.Position = @event.EffectPosition();
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 15000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Party Mesotes Connection Line Half Sec", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29266"],suppress:3000)]
    public void MesotesPartyConnected(Event @event, ScriptAccessory accessory)
    {
        if (isPartyMember(accessory, @event.SourceId()))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Party Mesotes Line{@event.SourceId()}";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = @event.EffectPosition();
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(1);
            dp.DestoryAt = 500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
        
    [ScriptMethod(name: "Party Mesotes Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3118"],userControl: false)]
    public void PartyMesotesDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Party Mesotes{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Enemy Mesotes Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29266"])]
    public void MesotesEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Mesotes{@event.SourceId()}";
            dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
            dp.Position = @event.EffectPosition();
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 15000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Enemy Mesotes Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3118"],userControl: false)]
    public void EnemyMesotesDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Enemy Mesotes{@event.SourceId()}");
    }
    
    #endregion

    #region Dark Knight
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Dark Knight â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void DarkKnight(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Enemy Salted Earth Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29094"],suppress:3000)]
    public void SaltedEarthEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Salted Earth{@event.SourceId()}";
            dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
            dp.Position = @event.EffectPosition();
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Enemy Salted Earth Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3036"],userControl: false)]
    public void EnemySaltedEarthDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Enemy Salted Earth{@event.SourceId()}");
    }
    
    #endregion

    #region Machinist
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Machinist â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Machinist(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Party Machinist Turret Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29412"],suppress:3000)]
    public void BishopAutoturretParty(Event @event, ScriptAccessory accessory)
    {
        if (isPartyMember(accessory, @event.SourceId()))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Party Bishop Autoturret{@event.SourceId()}";
            dp.Color = accessory.Data.DefaultSafeColor.WithW(0.6f);
            dp.Position = @event.EffectPosition();
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
        
    [ScriptMethod(name: "Party Machinist Turret Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3155"],userControl: false)]
    public void PartyMachinistTurretDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Party Bishop Autoturret{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Enemy Machinist Turret Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29412"],suppress:3000)]
    public void BishopAutoturretEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Bishop Autoturret{@event.SourceId()}";
            dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
            dp.Position = @event.EffectPosition();
            dp.Scale = new Vector2(5f);
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Enemy Machinist Turret Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3155"],userControl: false)]
    public void EnemyMachinistTurretDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Enemy Bishop Autoturret{@event.SourceId()}");
    }
    
    #endregion

    #region Paladin
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Paladin â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Paladin(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Cover Connection Line", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1301"])]
    public void CoverConnection(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cover Line";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(0f, 1f, 1f, 1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Cover Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1301"])]
    public void CoverRange(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cover Range";
        dp.Owner = @event.TargetId();
        dp.Color = new Vector4(0f, 1f, 1f, 0.15f);
        dp.Scale = new(10);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Cover Range Outline";
        dp1.Owner = @event.TargetId();
        dp1.Color = new Vector4(0f, 1f, 1f, 8f);
        dp1.Scale = new Vector2(10f);
        dp1.InnerScale = new Vector2(9.97f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "Covered Connection Line", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1301"])]
    public void CoveredConnectionLine(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        
        if (isText) accessory.Method.TextInfo($"Covered (by <{@event.SourceName()}>)", duration: 7300, false);
        if (isTTS)  accessory.Method.TTS("Covered");
        if (isEdgeTTS)  accessory.Method.EdgeTTS("Covered");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Covered Line{@event.SourceId()}";
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(1);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        accessory.Method.SendChat($"/e [Kodakku]: <{@event.SourceName()}> has covered you!");
    }
    
    [ScriptMethod(name: "Covered Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1301"])]
    public void CoveredRange(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Covered Range{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.25f);
        dp.Scale = new(10);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Covered Range Outline{@event.SourceId()}";
        dp1.Owner = @event.SourceId();
        dp1.Color = accessory.Data.DefaultSafeColor.WithW(8f);
        dp1.Scale = new Vector2(10f);
        dp1.InnerScale = new Vector2(9.97f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "Cover Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1300"],userControl: false)]
    public void CoverDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"Cover.*");
    }
    
    [ScriptMethod(name: "Covered Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1301"],userControl: false)]
    public void CoveredDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"Covered.*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Self Holy Sheltron (Shield Burst Range)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3026"])]
    public void HolySheltronSelf(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"HolySheltron Self";
        dp.Color = SelfAOEColor.V4;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Holy Sheltron Burst Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3026"],userControl: false)]
    public void HolySheltronDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"HolySheltron Self");
    }
    
    [ScriptMethod(name: "Enemy Holy Sheltron Range Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3026"])]
    public void HolySheltronEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Holy Sheltron{@event.SourceId()}";
            dp.Color = new Vector4(1f, 1f, 0f, 0.6f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(6f);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
        
    [ScriptMethod(name: "Enemy Holy Sheltron Destroy", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3026"],userControl: false)]
    public void EnemyHolySheltronDestroy(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Enemy Holy Sheltron{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Self Invincible Shows Confiteor 10m Range Outline", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1302"])]
    public void Confiteor(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Confiteor";
        dp.Color = SelfAOEColor.V4.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.InnerScale = new Vector2(9.9f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Confiteor Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:43243"],userControl: false)]
    public void ConfiteorDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"Confiteor.*");
    }
    
    #endregion

    #region Viper
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Viper â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Viper(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Self Armored Scales", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^409[78]$"])]
    public void ArmoredScales_Backlash_Self(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        switch (@event.StatusID())
        {
            case 4097:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"ArmoredScales Self";
                dp.Color = new Vector4(0f, 1f, 1f, 1f);
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(6f);
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;
            case 4098:
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"Snakeblood ArmoredScales Self";
                dp1.Color = new Vector4(0f, 1f, 1f, 0.6f);
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(15f);
                dp1.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                break;
        }
    }
    
    [ScriptMethod(name: "[Enable to hide] Hide 6m inner ring when self gains Snakeblood", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^4098$"])]
    public void SnakesBaneRemoveSelf(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"ArmoredScales Self");
    }
    
    [ScriptMethod(name: "Armored Scales Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^3918[78]$"],userControl: false)]
    public void ArmoredScales_Backlash_RemoveSelf(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($".*ArmoredScales Self");
    }
    
    [ScriptMethod(name: "Enemy Armored Scales Range Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^409[78]$"])]
    public void ArmoredScales_Backlash_Enmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            switch (@event.StatusID())
            {
                case 4097:
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"ArmoredScales Enemy{@event.SourceId}";
                    dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
                    dp.Owner = @event.SourceId();
                    dp.Scale = new Vector2(6f);
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
                case 4098:
                    var dp1 = accessory.Data.GetDefaultDrawProperties();
                    dp1.Name = $"Snakeblood ArmoredScales Enemy{@event.SourceId}";
                    dp1.Color = new Vector4(1f, 0f, 0f, 0.8f);
                    dp1.Owner = @event.SourceId();
                    dp1.Scale = new Vector2(15f);
                    dp1.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                    break;
            }
        }
    }
    
    [ScriptMethod(name: "[Enable to hide] Hide 6m inner ring when enemy gains Snakeblood", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^4098$"])]
    public void SnakesBaneRemoveEnmity(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"ArmoredScales Enemy{@event.SourceId}");
    }
        
    [ScriptMethod(name: "Enemy Armored Scales Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^3918[78]$"],userControl: false)]
    public void ArmoredScales_Backlash_RemoveEnmity(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*ArmoredScales Enemy{@event.SourceId()}");
    }

    #endregion

    #region Astrologian
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Astrologian â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Astrologian(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Enemy Celestial River Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29255"],suppress: 100)]
    public void CelestialRiverEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Celestial River{@event.SourceId()}";
            dp.Color = EnmityAOEColor.V4;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(15f);
            dp.DestoryAt = EnmityAOETimer;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    #endregion

    #region Bard
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Bard â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Bard(Event @event, ScriptAccessory accessory) { }
        
    [ScriptMethod(name: "Enemy Final Fantasia Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4312"])]
    public void FinalFantasiaEnmity(Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null || !obj.IsValid()) return;

        if (!PartyFilter(accessory, obj))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enemy Final Fantasia{@event.SourceId()}";
            dp.Color = EnmityAOEColor.V4;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(30f);
            dp.DestoryAt = EnmityAOETimer;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    #endregion

    #region Dancer
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Dancer â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Dancer(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Closed Position Prediction Outline [ Trigger Macro: /e CompulsoryControl ]", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^CompulsoryControl$"])]
    public void ClosedPositionPredictionOutline(Event @event, ScriptAccessory accessory)
    {
        if (IbcHelper.GetPlayerJob(accessory,accessory.Data.MyObject,false) != "DNC") return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Closed Position Prediction Outline";
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.9f);
        dp.Owner = accessory.Data.Me; 
        dp.Color = SelfCompulsoryControlColor.V4.WithW(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = MoveActionsTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Closed Position Prediction Fill [ Fill brightness in user settings ]", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^CompulsoryControl$"])]
    public void ClosedPositionPredictionFill(Event @event, ScriptAccessory accessory)
    {
        var myObject = accessory.Data.MyObject;
        if (myObject == null) return;
        var myJob = IbcHelper.GetPlayerJob(accessory, myObject, true);
        if (myJob != "Dancer") return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Closed Position Prediction Fill";
        dp.Scale = new Vector2(15f);
        dp.Owner = accessory.Data.Me; 
        dp.Color = SelfCompulsoryControlColor.V4.WithW(SelfCompulsoryControlFillBrightness);
        dp.DestoryAt = MoveActionsTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Closed Position Trigger Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^29432$"],userControl: false)]
    public void ClosedPositionDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"Closed Position Prediction.*");
    }

    #endregion
    
    #region Reaper
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” Reaper â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Reaper(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Arcane Circle Prediction Outline [ Trigger Macro: /e CompulsoryControl ]", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^CompulsoryControl$"])]
    public void ArcaneCirclePredictionOutline(Event @event, ScriptAccessory accessory)
    {
        var myObject = accessory.Data.MyObject;
        if (myObject == null) return;
        var myJob = IbcHelper.GetPlayerJob(accessory, myObject, true);
        if (myJob != "Reaper") return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Arcane Circle Prediction Outline";
        dp.Scale = new Vector2(10f);
        dp.InnerScale = new Vector2(9.94f);
        dp.Owner = accessory.Data.Me;
        dp.Color = SelfCompulsoryControlColor.V4.WithW(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = MoveActionsTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
}
    
    [ScriptMethod(name: "Arcane Circle Prediction Fill [ Fill brightness in user settings ]", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^CompulsoryControl$"])]
    public void ArcaneCirclePredictionFill(Event @event, ScriptAccessory accessory)
    {
        if (IbcHelper.GetPlayerJob(accessory,accessory.Data.MyObject,false) != "RPR") return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Arcane Circle Prediction Fill";
        dp.Scale = new Vector2(10f);
        dp.Owner = accessory.Data.Me; 
        dp.Color = SelfCompulsoryControlColor.V4.WithW(SelfCompulsoryControlFillBrightness);
        dp.DestoryAt = MoveActionsTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Arcane Circle Trigger Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^29553$"],userControl: false)]
    public void ArcaneCircleDestroy(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($"Arcane Circle Prediction.*");
    }

    #endregion
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

    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }

    public static uint SourceDataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);
    }

    public static uint Command(this Event @event)
    {
        return ParseHexId(@event["Command"], out var cid) ? cid : 0;
    }
    
    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
    }

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static float TargetRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["TargetRotation"]);
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
        return @event["SourceName"];
    }

    public static string TargetName(this Event @event)
    {
        return @event["TargetName"];
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

    public static uint DirectorId(this Event @event)
    {
        return ParseHexId(@event["DirectorId"], out var id) ? id : 0;
    }

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
    }

    public static uint StackCount(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StackCount"]);
    }

    public static uint Param(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["Param"]);
    }
}

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class IbcHelper
{
    public static string GetPlayerJob(this ScriptAccessory accessory, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    public static string GetPlayerRole(this ScriptAccessory sa, IPlayerCharacter? playerObject)
    {
        if (playerObject == null) return "None";
        return playerObject.ClassJob.Value.Role switch
        {
            1 => "Tank",
            4 => "Healer",
            2 => "Melee DPS",
            3 => "Ranged DPS",
            _ => "Unknown"
        };
    }
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;
            
        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "Attack1",
            MarkType.Attack2 => "Attack2",
            MarkType.Attack3 => "Attack3",
            MarkType.Attack4 => "Attack4",
            MarkType.Attack5 => "Attack5",
            MarkType.Bind1 => "Bind1",
            MarkType.Bind2 => "Bind2",
            MarkType.Bind3 => "Bind3",
            MarkType.Ignore1 => "Ignore1",
            MarkType.Ignore2 => "Ignore2",
            MarkType.Square => "Square",
            MarkType.Circle => "Circle",
            MarkType.Cross => "Cross",
            MarkType.Triangle => "Triangle",
            MarkType.Attack6 => "Attack6",
            MarkType.Attack7 => "Attack7",
            MarkType.Attack8 => "Attack8",
            _ => "No Marker"
        };
    }
}

public static class ActionExt
{
    public static unsafe bool IsReadyWithCanCast(uint actionId, ActionType actionType)
    {
        var am = ActionManager.Instance();
        if (am == null) return false;

        var adjustedId = am->GetAdjustedActionId(actionId);

        if (am->GetActionStatus(actionType, adjustedId) != 0)
            return false;

        ulong targetId = 0;
        var ts = TargetSystem.Instance();
        if (ts != null && ts->GetTargetObject() != null)
            targetId = ts->GetTargetObject()->GetGameObjectId();

        return am->GetActionStatus(actionType, adjustedId, targetId) == 0;
    }

    public static bool IsSpellReady(this uint spellId) => IsReadyWithCanCast(spellId, ActionType.Action);
}
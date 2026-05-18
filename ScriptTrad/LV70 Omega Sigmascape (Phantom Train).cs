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
using System.Threading.Tasks;

namespace O5n;

[ScriptType(guid: "50a5cdbc-9894-4dd7-97a1-0d1b3ae67bc7", name: "O5N", territorys: [748],
    version: "0.0.0.2", author: "Linoa235", note: noteStr)]

public class O5n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 Omega: Sigmascape (Phantom Train) Initial Drawing
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    uint Ghost = 0;
    
    public void Init(ScriptAccessory accessory) {
        Ghost = 0;
    }
    
    [ScriptMethod(name: "Ghost Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10405"], userControl: false)]
    public void GhostRecord(Event @event, ScriptAccessory accessory)
    {
        Ghost = 1;
    }
    
    [ScriptMethod(name: "Tail Chase Anti-Knockback Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10415"])]
    public async void TailChase(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(2000); 
        if (isText) accessory.Method.TextInfo("Use anti-knockback", duration: 1500, false);
        if (isTTS) accessory.Method.EdgeTTS("Use anti-knockback");
    }
    
    [ScriptMethod(name: "Holy Ray Marker Hint", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public async void HolyRayMarker(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (Ghost != 1 && isText) accessory.Method.TextInfo("Holy Ray marker", duration: 3000, true);
        if (Ghost != 1 && isTTS) accessory.Method.EdgeTTS("Holy Ray marker");
        if (Ghost == 1 && isText) accessory.Method.TextInfo("Place AOE under the ghost", duration: 3000, true);
        if (Ghost == 1 && isTTS) accessory.Method.EdgeTTS("Place AOE under the ghost");
        
        await Task.Delay(8000);
        if (isTTS) accessory.Method.EdgeTTS("Get away");
        
        Ghost = 0;
    }
    
    [ScriptMethod(name: "Ghost Tether Hint", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0001"])]
    public void GhostTether(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Guide the ghost to the light", duration: 3000, true);
        if (isTTS) accessory.Method.EdgeTTS("Guide the ghost to the light");
    }
    
    [ScriptMethod(name: "Ghost Highlight", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:8511"])]
    public void GhostHighlight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ghost";
        dp.Color = new Vector4(0f, 1f, 1f, 6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.86f);
        dp.InnerScale = new Vector2(1.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 18000;
        if (Ghost == 1) accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Ghost Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:8511"], userControl: false)]
    public void GhostCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Ghost");
    }
    
    [ScriptMethod(name: "Nether Headlight (Line Stack)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:10989"])]
    public void NetherHeadlight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Nether Headlight";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.5f);
        dp.Scale = new (6f, 65.8f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Nuke Placement Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1253"])]
    public async void NukePlacementHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Place the nuke at the back", duration: 7500, true);
        if (isTTS) accessory.Method.EdgeTTS("Place the nuke at the back");
        
        await Task.Delay(8000);
        if (isTTS) accessory.Method.EdgeTTS("Get away");
    }
    
    [ScriptMethod(name: "Suffocation Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:938"])]
    public void Suffocation(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Grabbed by ghost", duration: 2000, true);
        if (isTTS) accessory.Method.EdgeTTS("Grabbed by ghost");
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

    public static uint StatusId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusId"]);
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

public static class Extensions
{
    public static void TTS(this ScriptAccessory accessory, string text, bool isTTS, bool isDRTTS)
    {
        if (isDRTTS)
        {
            accessory.Method.SendChat($"/pdr tts {text}");
        }
        else if (isTTS)
        {
            accessory.Method.TTS(text);
        }
    }
}
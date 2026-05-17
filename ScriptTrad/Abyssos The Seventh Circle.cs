using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using Dalamud.Memory.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;
using Newtonsoft.Json.Linq;

namespace CicerosKodakkuAssist.Pandemonium.Normal;

[ScriptType(name: "Abyssos The Seventh Circle", territorys: [], $179e4d9da-ee30-4e83-ad56-67899d060054", version: "0.0.0.1", Author: "Linoa235", guid: "64bc5200-d5e3-4166-9dfd-820140bcc31f")]

public class Abyssos_The_Seventh_Circle
{
    [UserSetting("Enable Text Prompts")]
    public bool Enable_Text_Prompts { get; set; } = true;
    
    [UserSetting("Text Prompt Language")]
    public Languages_Of_Text_Prompts Language_Of_Text_Prompts { get; set; }
    
    [UserSetting("Enable Developer Mode")]
    public bool Enable_Developer_Mode { get; set; } = false;

    public enum Languages_Of_Text_Prompts
    {
        Simplified_Chinese,
        English
    }
    
    [ScriptMethod(name: "Bough of Attis (Front)",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:30714"])]
    public void Bough_of_Attis_Front(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = sourceId;
        currentProperty.Scale = new(19);
        currentProperty.DestoryAt = 7700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

        if (Enable_Text_Prompts)
        {
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                accessory.Method.TextInfo("è¿œç¦»Boss", 2500);
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                accessory.Method.TextInfo("Stay away from the Boss", 2500);
        }
    }
    
    [ScriptMethod(name: "Bough of Attis (Back)",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:30719"])]
    public void Bough_of_Attis_Back(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = sourceId;
        currentProperty.Scale = new(25);
        currentProperty.DestoryAt = 7700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
        
        if (Enable_Text_Prompts)
        {
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                accessory.Method.TextInfo("é è¿‘Boss", 2500);
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                accessory.Method.TextInfo("Approach the Boss", 2500);
        }
    }
    
    [ScriptMethod(name: "Bough of Attis (Side)",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:30717"])]
    public void Bough_of_Attis_Side(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        var effectPositionInJson = JObject.Parse(@event["EffectPosition"]);
        float currentX = effectPositionInJson["X"]?.Value<float>() ?? 0;
        
        currentProperty.Owner = sourceId;
        currentProperty.Offset = new Vector3(0, 0, 10);
        currentProperty.Scale = new(25, 50);
        currentProperty.DestoryAt = 4700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        
        if (Enable_Text_Prompts)
        {
            if (currentX < 100)
            {
                if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                    accessory.Method.TextInfo("åŽ»å³è¾¹èº²é¿", 1500);
                if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                    accessory.Method.TextInfo("Dodge on the right", 1500);
            }
            if (currentX > 100)
            {
                if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                    accessory.Method.TextInfo("åŽ»å·¦è¾¹èº²é¿", 1500);
                if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                    accessory.Method.TextInfo("Dodge on the left", 1500);
            }
        }
        
        if (Enable_Developer_Mode)
        {
            accessory.Method.SendChat($"/e @event[\"EffectPosition\"]={@event["EffectPosition"]} currentX={currentX}");
        }
    }
    
    [ScriptMethod(name: "Static Moon",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:30722"])]
    public void Static_Moon(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = sourceId;
        currentProperty.Scale = new(10);
        currentProperty.DestoryAt = 4700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
        
        if (Enable_Text_Prompts)
        {
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                accessory.Method.TextInfo("è¿œç¦»è´å¸Œæ‘©æ–¯", 1500);
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                accessory.Method.TextInfo("Stay away from behemoths", 1500);
        }
    }
    
    [ScriptMethod(name: "Stymphalian Strike",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:30723"])]
    public void Stymphalian_Strike(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = sourceId;
        currentProperty.Scale = new(8, 60);
        currentProperty.DestoryAt = 4700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        
        if (Enable_Text_Prompts)
        {
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                accessory.Method.TextInfo("è¿œç¦»æ€ªé¸Ÿæ­£é¢", 1500);
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                accessory.Method.TextInfo("Stay away from the front of birds", 1500);
        }
    }
    
    [ScriptMethod(name: "Blades of Attis",
        eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(30725|30726)$"])]
    public void Blades_of_Attis(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = sourceId;
        currentProperty.Offset = new Vector3(0, 0, -8);
        currentProperty.Scale = new(7);
        currentProperty.DestoryAt = 1250;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

        if (Enable_Text_Prompts)
        {
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                accessory.Method.TextInfo("èº²é¿æ­¥è¿›å¼AOE", 9000);
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                accessory.Method.TextInfo("Dodge stepping AOEs", 9000);
        }
    }
    
    [ScriptMethod(name: "Hemitheos's Aero IV",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:30785"])]
    public void Hemitheoss_Aero_IV(Event @event, ScriptAccessory accessory)
    {
        if (!parseObjectId(@event["SourceId"], out var sourceId)) return;
        
        var currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = sourceId;
        currentProperty.TargetObject = accessory.Data.Me;
        currentProperty.ScaleMode |= ScaleMode.YByDistance;
        currentProperty.Scale = new(1.5f);
        currentProperty.DestoryAt = 6700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, currentProperty);
        
        currentProperty = accessory.Data.GetDefaultDrawProperties();
        currentProperty.Owner = accessory.Data.Me;
        currentProperty.TargetObject = sourceId;
        currentProperty.Rotation = float.Pi;
        currentProperty.Scale = new(1.5f, 25);
        currentProperty.DestoryAt = 6700;
        currentProperty.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, currentProperty);
        
        if (Enable_Text_Prompts)
        {
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.Simplified_Chinese)
                accessory.Method.TextInfo("å‡»é€€", 2000);
            if (Language_Of_Text_Prompts == Languages_Of_Text_Prompts.English)
                accessory.Method.TextInfo("Knock back", 2000);
        }
    }
    
    private static bool parseObjectId(string? idStr, out ulong id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            var idStr2 = idStr.Replace("0x", "");
            id = ulong.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
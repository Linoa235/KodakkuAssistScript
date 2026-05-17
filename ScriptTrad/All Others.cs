using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InteropGenerator.Runtime.Attributes;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Lumina.Excel.Sheets;
using Lumina.Excel.Sheets.Experimental;
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
using System.Xml.Linq;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.DataCenterHelper;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Veever.Other.VVToolKit;

/// <summary>
/// Party filter mode
/// </summary>
public enum PartyFilterMode
{
    [Description("Disable filtering")]
    Disabled = 0,
    [Description("Draw only my party")]
    OnlyMyParty = 1,
    [Description("Exclude my party")]
    ExcludeMyParty = 2
}

[ScriptType(name: "All Others", territorys: [], $128463427-3299-4f90-9a24-25708dd45450", version: "0.0.0.1", Author: "Linoa235", guid: "b46a739b-f50f-4ea5-8fc9-7542979dbe01")]

public class VVToolKit
{
    const string NoteStr =
    """ 
    v0.0.2.4
    ---------------------------------------------------------
    1. Enter /e vvfind + name; automatically finds the person you're looking for within range
    2. To turn off navigation markers or other extra features, enter /e vvstop
    3. Enter /e vvmove or /e vvfly to call vnav to go to a specified position (can also be triggered in method settings)
    4. /e vvguid automatically generates a new GUID and copies it to clipboard
    5. /e vvrot changes your own facing to the target's current facing (need to find target first using vvfind or vvselect, local only)
    6. /e vvrotme [number] changes your own facing to a specified radian value
    7. /e vvrottarget sets the target object's (player/pet/NPC) facing to your current facing (need to find target first using vvfind or vvselect, local only)
    8. /e vvrottarget [number] sets the target object's (player/pet/NPC) facing to a specified radian (need to find target first using vvfind or vvselect, local only)
    9. /e vvlist lists up to 10 nearby players
    10. /e vvselect [number] selects a player listed by /vvlist as target
    11. /e vvvvv authentication key, required to use additional features
    ---------------------------------------------------------
    The following features require a key:
    1. /e vvtp

    Duck Sect
    """;

    const string UpdateInfo =
    """
        v0.0.2.4
        vvrottarget supports adding parameter to set specified facing
        Added vvlist to display nearby players
        Added vvselect to select a specific player from vvlist as target
        Added vvrot to change own facing to target's current facing
        Added vvrotme to change own facing to specified radian
    """;

    private const string Name = "VV Toolbox";
    private const string Version = "0.0.2.4";
    private const string DebugVersion = "a";

    private const bool Debugging = true;

    [UserSetting("Authentication Key")]
    public string key { get; set; } = "123";

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("Display tracking line")]
    public bool isDraw { get; set; } = true;

    [UserSetting("Enable target type filtering")]
    public bool OnlyFindTypeTrigger { get; set; } = true;

    [UserSetting("Target type to search for")]
    public Dalamud.Game.ClientState.Objects.Enums.ObjectKind OnlyFindType { get; set; } = Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("Output results to chat")]
    public bool isChat { get; set; } = false;

    [UserSetting("FFLog")]
    public bool isFFLog { get; set; } = true;

    [UserSetting("Rising Stones")]
    public bool isStone { get; set; } = true;

    public bool isOpenBox { get; set; } = false;

    public string name = "";
    public string findNameFramework = "";
    public bool FindTargetObjectFramework = false;
    // The key is plaintext, don't look at the spaghetti code below QwQ
    public string mainKey = "A-qdPv6??Hgr9sKyAYjXa.W~WigeEEVt2LF7pnr15QteK1ynF2e-Urh:MxCt@t,]:DmG-CMqmBCzLg^7+~#8sRP*pnX?wofsXHN4";

    private readonly object findLock = new object();
    private List<IGameObject> cachedPlayers = new();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isChat) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }
    
    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        sa.Method.ClearFrameworkUpdateAction(this);
        name = "";
    }


    [ScriptMethod(name: "Verify Key", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvvvv$"])]
    public void verifyKey(Event ev, ScriptAccessory sa)
    {
        if (key == mainKey)
        {
            isOpenBox = true;
            sa.Log.Debug("Key verified, OpenBox!");
            sa.Method.TextInfo("Key verification successful", 4700);
        } else
        {
            sa.Method.TextInfo("Key verification failed", 4700, true);
        }
    }

    public IGameObject? ob;

    [ScriptMethod(name: "FindTarget", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvfind (.*)$"])]
    public void FindTargetObject(Event ev, ScriptAccessory sa)
    {
        string testname = ev["Message"];
        sa.Log.Debug($"get message: {testname}");

        if (testname.StartsWith("vvfind "))
        {
            string targetName = testname.Substring(7);
            name = targetName;
            sa.Log.Debug($"Name: {targetName}");

            findNameFramework = sa.Method.RegistFrameworkUpdateAction(() =>
            {

                var findingObj = sa.Data.Objects?.Where(x => x.Name.ToString() == name).FirstOrDefault();


                if (findingObj == null) return;
                if (OnlyFindTypeTrigger && findingObj.ObjectKind != OnlyFindType) return;

                ob = findingObj;

                FindTargetObjectFramework = true;

                FindTargetDetail(ev, sa, findingObj);
            });

        }
    }

    [ScriptMethod(name: "StopFindTarget", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvstop$"])]
    public void StopFindTargetObject(Event ev, ScriptAccessory sa)
    {
        sa.Method.SendChat("/e STOP");
        sa.Method.ClearFrameworkUpdateAction(this);
        sa.Method.RemoveDraw($"Target Search");
        sa.Method.SendChat($"/vnav stop");
    }

    [ScriptMethod(name: "Generate GUID", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvguid$"])]
    public void vvGuid(Event ev, ScriptAccessory sa)
    {
        Guid guid = Guid.NewGuid();
        string guidString = guid.ToString();
        
        bool success = false;
        try
        {
            var thread = new Thread(() =>
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(guidString);
                    success = true;
                }
                catch (Exception ex)
                {
                    sa.Log.Error($"Clipboard thread internal error: {ex.Message}");
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (success)
            {
                sa.Method.SendChat($"/e {guidString} (copied to clipboard)");
                sa.Log.Debug($"GUID copied to clipboard: {guidString}");
            }
            else
            {
                sa.Method.SendChat($"/e {guidString} (copy failed)");
            }
        }
        catch (Exception ex)
        {
            sa.Method.SendChat($"/e {guidString} (copy failed)");
            sa.Log.Error($"Failed to copy to clipboard: {ex.Message}");
        }
    }

    [ScriptMethod(name: "VnavMove", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvmove$"])]
    public void vnavMove(Event ev, ScriptAccessory sa)
    {
        if (ob == null)
        {
            sa.Method.SendChat("/e Target object not found, please use /e vvfind first");
            return;
        }

        var x = ob.Position.X;
        var y = ob.Position.Y;
        var z = ob.Position.Z;
        sa.Method.SendChat($"/vnav moveto {x} {y} {z}");
    }

    [ScriptMethod(name: "VnavFly", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvfly$"])]
    public void vnavFly(Event ev, ScriptAccessory sa)
    {
        if (ob == null)
        {
            sa.Method.SendChat("/e Target object not found, please use /e vvfind first");
            return;
        }

        var x = ob.Position.X;
        var y = ob.Position.Y;
        var z = ob.Position.Z;
        sa.Method.SendChat($"/vnav flyto {x} {y} {z}");
    }


    [ScriptMethod(name: "Teleport (do not use in the wild!!)", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvtp$"])]
    public void teleportOb(Event ev, ScriptAccessory sa)
    {
        if (key != mainKey) return;
        if (ob == null) return;
        SpecialFunction.SetPosition(sa, sa.Data.MyObject, ob.Position);
    }


    [ScriptMethod(name: "Set Self Rotation to Target", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvrot$"])]
    public void setRot(Event ev, ScriptAccessory sa)
    {
        var myobj = sa.Data.MyObject;
        if (myobj == null) return;

        if (ob == null || !ob.IsValid() || myobj == null || !myobj.IsValid())
        {
            sa.Log.Error($"Invalid Object");
            return;
        }

        unsafe
        {
            GameObject* charaStruct = (GameObject*)myobj.Address;
            charaStruct->SetRotation(ob.Rotation);
        }
        sa.Log.Debug($"Changed facing {myobj.Name.TextValue} | {myobj.Rotation.RadToDeg()} => obj: {ob.Name.TextValue} {ob.Rotation.RadToDeg()}");
    }

    [ScriptMethod(name: "Set Target Rotation", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvrottarget( (.+))?$"])]
    public void setTargetRot(Event ev, ScriptAccessory sa)
    {
        var myobj = sa.Data.MyObject;
        if (myobj == null)
        {
            sa.Method.SendChat("/e Unable to get player object");
            sa.Log.Error("MyObject is null");
            return;
        }

        if (ob == null || !ob.IsValid())
        {
            sa.Method.SendChat("/e Target object invalid, please use /e vvfind first");
            sa.Log.Error("ob is null or invalid");
            return;
        }

        sa.Log.Debug($"Target object: {ob.Name.TextValue}, ObjectKind: {ob.ObjectKind}, EntityId: {ob.EntityId}");

        string message = ev["Message"];
        float newRotation;
        bool useCustomRotation = false;

        if (message.Length > 11 && message.Substring(11).Trim().Length > 0)
        {
            string input = message.Substring(11).Trim();
            if (float.TryParse(input, out float rotation))
            {
                newRotation = rotation;
                useCustomRotation = true;
            }
            else
            {
                sa.Method.SendChat("/e Invalid parameter, please enter a valid number");
                sa.Log.Error($"Invalid rotation value: {input}");
                return;
            }
        }
        else
        {
            newRotation = myobj.Rotation;
        }

        unsafe
        {
            GameObject* targetStruct = (GameObject*)ob.Address;
            float oldRotation = targetStruct->Rotation;

            targetStruct->SetRotation(newRotation);

            if (useCustomRotation)
            {
                sa.Log.Debug($"Changed target facing {ob.Name.TextValue} ({ob.ObjectKind}) | {oldRotation.RadToDeg()}° => specified facing: {newRotation.RadToDeg()}°");
                sa.Method.SendChat($"/e Target {ob.Name.TextValue} facing set to {newRotation} radians ({newRotation.RadToDeg():F1}°)");
            }
            else
            {
                sa.Log.Debug($"Changed target facing {ob.Name.TextValue} ({ob.ObjectKind}) | {oldRotation.RadToDeg()}° => player facing: {newRotation.RadToDeg()}°");
                sa.Method.SendChat($"/e Target {ob.Name.TextValue} facing set to match yours ({newRotation.RadToDeg():F1}°)");
            }
        }
    }

    [ScriptMethod(name: "Set Self Rotation to Value", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvrotme (.+)$"])]
    public void setMyRot(Event ev, ScriptAccessory sa)
    {
        var myobj = sa.Data.MyObject;
        if (myobj == null)
        {
            sa.Method.SendChat("/e Unable to get player object");
            sa.Log.Error("MyObject is null");
            return;
        }

        string input = ev["Message"].Substring(8).Trim();
        if (!float.TryParse(input, out float rotation))
        {
            sa.Method.SendChat("/e Invalid parameter, please enter a valid number");
            sa.Log.Error($"Invalid rotation value: {input}");
            return;
        }

        unsafe
        {
            GameObject* charaStruct = (GameObject*)myobj.Address;
            float oldRotation = charaStruct->Rotation;

            charaStruct->SetRotation(rotation);

            sa.Log.Debug($"Changed self facing {myobj.Name.TextValue} | {oldRotation.RadToDeg()}° => {rotation.RadToDeg()}°");
            sa.Method.SendChat($"/e Self facing set to {rotation} radians ({rotation.RadToDeg():F1}°)");
        }
    }

    [ScriptMethod(name: "List Nearby Players", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvlist$"])]
    public async void listPlayers(Event ev, ScriptAccessory sa)
    {
        var myobj = sa.Data.MyObject;
        if (myobj == null)
        {
            sa.Method.SendChat("/e Unable to get player position");
            return;
        }

        lock (findLock)
        {
            cachedPlayers = sa.Data.Objects?
                .Where(x => x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player)
                .OrderBy(x => Vector3.Distance(myobj.Position, x.Position))
                .Take(10)
                .ToList() ?? new();
        }

        if (cachedPlayers.Count == 0)
        {
            sa.Method.SendChat("/e No nearby players found");
            return;
        }

        sa.Method.SendChat("/e === Nearby players (use /e vvselect <number> to select) ===");
        await Task.Delay(50);

        for (int i = 0; i < cachedPlayers.Count; i++)
        {
            var distance = Vector3.Distance(myobj.Position, cachedPlayers[i].Position);
            sa.Method.SendChat($"/e [{i + 1}] {cachedPlayers[i].Name} ({distance:F1}m)");
            await Task.Delay(50);
        }

        sa.Log.Debug($"Listed {cachedPlayers.Count} nearby players");
    }

    [ScriptMethod(name: "Select Player", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^vvselect (\\d+)$"])]
    public void selectPlayer(Event ev, ScriptAccessory sa)
    {
        string input = ev["Message"].Substring(9).Trim();

        lock (findLock)
        {
            if (!int.TryParse(input, out int index) || index < 1 || index > cachedPlayers.Count)
            {
                sa.Method.SendChat("/e Invalid number, please use /e vvlist first");
                sa.Log.Error($"Invalid index: {input}, cachedPlayers count: {cachedPlayers.Count}");
                return;
            }

            ob = cachedPlayers[index - 1];
            if (ob == null || !ob.IsValid())
            {
                sa.Method.SendChat("/e Player object invalid");
                sa.Log.Error("Selected player object is null or invalid");
                return;
            }

            sa.Method.SendChat($"/e Selected: {ob.Name}");
            sa.Log.Debug($"Selected player: {ob.Name} (EntityId: {ob.EntityId})");
        }
    }

    public void FindTargetDetail(Event ev, ScriptAccessory sa, IGameObject? findingObj)
    {
        // sa.Method.RemoveDraw("Target Search");
        //
        // await Task.Delay(1100);

        lock (findLock)
        {

            if (!FindTargetObjectFramework) return;

            sa.Method.UnregistFrameworkUpdateAction(findNameFramework);
            sa.Method.ClearFrameworkUpdateAction(this);

            FindTargetObjectFramework = false;

            if (findingObj == null) return;
            

            var finding = findingObj.Name;


            sa.Log.Debug($"found object: {finding}, position: {findingObj.Position.Quantized()}");

            if (isText) sa.Method.TextInfo($"Found target {finding}, position: {findingObj.Position.Quantized()}", 5700);
            if (isChat) sa.Method.SendChat($"/e Found target {finding}, position: {findingObj.Position.Quantized()}");
            if (isTTS) sa.Method.EdgeTTS($"Found target {finding}");
            if (isOpenBox && findingObj.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player)
            {
                sa.Log.Debug("Non-player, cannot open info box");
            }

            if (isOpenBox && findingObj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player)
            {
                unsafe
                {
                    var address = (Character*)findingObj.Address;
                    var Health = address->CharacterData.Health;
                    var Mana = address->CharacterData.Mana;
                    var job = IbcHelper.GetPlayerJob(sa, (IPlayerCharacter)findingObj, true);
                    var level = address->CharacterData.Level;
                    var homeworldString = ((IPlayerCharacter)findingObj).HomeWorld.Value.Name.ToString();

                    var StatusFlags = ((IPlayerCharacter)findingObj).StatusFlags;
                    var cid = address->ContentId;
                    var Height = address->Height;

                    var sex = address->Sex;
                    var sexStr = "";
                    if (sex == 1)
                    {
                        sexStr = "Female";
                    }
                    else
                    {
                        sexStr = "Male";
                    }


                    sa.Log.Debug($"\ncid: {cid},\nHp: {Health},\nMp: {Mana},\n" +
                        $"job: {job},\nlevel: {level}," +
                        $"\nsex: {sexStr},\nHomeWorld: {homeworldString}\n" +
                        $"StatusFlags: {StatusFlags}");

                    if (isStone)
                    {
                        _ = RisingStonHelper.SearchRisingStone(sa, (IPlayerCharacter)findingObj);
                    }

                    if (isFFLog) FFLogsHelper.OpenFFLogs(sa, (IPlayerCharacter)findingObj);
                }


                if (isChat) sa.Method.SendChat($"/e Found target {finding}, position: {findingObj.Position.Quantized()}");
            }

            var dp1 = sa.Data.GetDefaultDrawProperties();
            dp1.Name = $"Target Search";
            dp1.Owner = sa.Data.Me;
            dp1.Color = sa.Data.DefaultSafeColor;
            dp1.ScaleMode |= ScaleMode.YByDistance;
            dp1.TargetObject = (ulong)findingObj.GameObjectId;
            sa.Log.Debug($"EntityId: {(ulong)findingObj.EntityId}");
            dp1.Scale = new(2);
            dp1.DestoryAt = int.MaxValue;
            dp1.FixRotation = true;
            if (isDraw) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);

            sa.Method.ClearFrameworkUpdateAction(this);
        }
    }


}


public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

}


    #region Function Set
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

    public static uint Index(this Event ev)
    {
        return ParseHexId(ev["Index"], out var index) ? index : 0;
    }
}

public static class IbcHelper
{
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

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
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

    /// <summary>
    /// Get the marker/sign on an object
    /// </summary>
    /// <param name="obj">Game object</param>
    /// <returns>Marker type</returns>
    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        unsafe
        {
            // Read marker info directly from GameObject structure
            var gameObj = (GameObject*)obj.Address;
            if (gameObj == null) return MarkType.None;

            // GameObject.NamePlateIconId stores the head marker ID
            // Marker ID range: 0 = no marker, 1-8 = attack markers, 61-63 = bind markers, etc.
            var iconId = gameObj->NamePlateIconId;

            // Convert iconId to MarkType
            return iconId switch
            {
                0 => MarkType.None,
                61 => MarkType.Attack1,
                62 => MarkType.Attack2,
                63 => MarkType.Attack3,
                64 => MarkType.Attack4,
                65 => MarkType.Attack5,
                66 => MarkType.Bind1,
                67 => MarkType.Bind2,
                68 => MarkType.Bind3,
                69 => MarkType.Ignore1,
                70 => MarkType.Ignore2,
                71 => MarkType.Square,
                72 => MarkType.Circle,
                73 => MarkType.Cross,
                74 => MarkType.Triangle,
                75 => MarkType.Attack6,
                76 => MarkType.Attack7,
                77 => MarkType.Attack8,
                _ => MarkType.None
            };
        }
    }

    /// <summary>
    /// Check if an object has a specific marker
    /// </summary>
    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    /// <summary>
    /// Get the marker name
    /// </summary>
    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "Attack 1",
            MarkType.Attack2 => "Attack 2",
            MarkType.Attack3 => "Attack 3",
            MarkType.Attack4 => "Attack 4",
            MarkType.Attack5 => "Attack 5",
            MarkType.Bind1 => "Bind 1",
            MarkType.Bind2 => "Bind 2",
            MarkType.Bind3 => "Bind 3",
            MarkType.Ignore1 => "Ignore 1",
            MarkType.Ignore2 => "Ignore 2",
            MarkType.Square => "Square",
            MarkType.Circle => "Circle",
            MarkType.Cross => "Cross",
            MarkType.Triangle => "Triangle",
            MarkType.Attack6 => "Attack 6",
            MarkType.Attack7 => "Attack 7",
            MarkType.Attack8 => "Attack 8",
            _ => "No marker"
        };
    }
}

/// <summary>
/// Marker type enumeration
/// </summary>
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

#region Special Functions

public static class SpecialFunction
{
    public static void SetTargetable(this ScriptAccessory sa, IGameObject? obj, bool targetable)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"The provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            if (targetable)
            {
                if (obj.IsDead || obj.IsTargetable) return;
                charaStruct->TargetableStatus |= ObjectTargetableFlags.IsTargetable;
            }
            else
            {
                if (!obj.IsTargetable) return;
                charaStruct->TargetableStatus &= ~ObjectTargetableFlags.IsTargetable;
            }
        }
        sa.Log.Debug($"SetTargetable {targetable} => {obj.Name} {obj}");
    }

    public static void ScaleModify(this ScriptAccessory sa, IGameObject? obj, float scale)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"The provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->Scale = scale;
            charaStruct->DisableDraw();
            charaStruct->EnableDraw();
        }
        sa.Log.Debug($"ScaleModify => {obj.Name.TextValue} | {obj} => {scale}");
    }

    public static void SetPosition(this ScriptAccessory sa, IGameObject? obj, Vector3 position, bool show = false)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"The provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetPosition(position.X, position.Y, position.Z);
        }
        sa.Log.Debug($"Position changed => {obj.Name.TextValue} | {obj.EntityId} => {position}");

        if (!show) return;
    }
}

#endregion Special Functions


public static class NamazuHelper
{
    public class NamazuCommand(ScriptAccessory accessory, string url, string command, string param)
    {
        private ScriptAccessory accessory { get; set; } = accessory;
        private string _url = url;

        public void PostCommand()
        {
            var url = $"{_url}/{command}";
            accessory.Log.Debug($"Sending {param} to {url}");
            accessory.Method.HttpPost(url, param);
        }
    }

    public class Waymark
    {
        public ScriptAccessory accessory { get; set; }
        private Dictionary<string, object> _jsonObj = new();
        private string? _jsonPayload;

        public Waymark(ScriptAccessory _accessory)
        {
            accessory = _accessory;
        }

        public void AddWaymarkType(string type, Vector3 pos, bool active = true)
        {
            string[] validTypes = ["A", "B", "C", "D", "One", "Two", "Three", "Four"];
            var waymarkType = type;
            if (!validTypes.Contains(type)) return;
            _jsonObj[waymarkType] = new Dictionary<string, object>
            {
                { "X", pos.X },
                { "Y", pos.Y },
                { "Z", pos.Z },
                { "Active", active }
            };
        }

        public void SetJsonPayload(bool local = true, bool log = true)
        {
            _jsonObj["LocalOnly"] = local;
            _jsonObj["Log"] = log;
            _jsonPayload = JsonConvert.SerializeObject(_jsonObj);
        }

        public string? GetJsonPayload()
        {
            if (_jsonPayload == null)
                SetJsonPayload();
            return _jsonPayload;
        }

        public void PostWaymarkCommand(int port)
        {
            var param = GetJsonPayload();
            if (param == null) return;
            var post = new NamazuCommand(accessory, $"http://127.0.0.1:{port}", "place", param);
            post.PostCommand();
        }

        public void ClearWaymarks(int port)
        {
            var post = new NamazuCommand(accessory, $"http://127.0.0.1:{port}", "place", "clear");
            post.PostCommand();
        }
    }
}

public static class DrawHelper
{
    public static void DrawBeam(ScriptAccessory accessory, Vector3 sourcePosition, Vector3 targetPosition, string name = "Light's Course", int duration = 6700, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = sourcePosition;
        dp.TargetPosition = targetPosition;
        dp.Scale = new Vector2(10, 50);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawCircle(ScriptAccessory accessory, Vector3 position, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Circle, dp);
    }

    public static void DrawDisplacement(ScriptAccessory accessory, Vector3 targetPos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = targetPos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    public static void DrawDisplacementby2points(ScriptAccessory accessory, Vector3 origin, Vector3 target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Position = origin;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = target;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    public static void DrawDisplacementObject(ScriptAccessory accessory, ulong target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false, DrawModeEnum drawmode = DrawModeEnum.Imgui)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = target;
        dp.Scale = scale;
        dp.Delay = delay; 
        dp.FixRotation = fix;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Displacement, dp);
    }

    public static void DrawRect(ScriptAccessory accessory, Vector3 position, Vector3 targetPos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.TargetPosition = targetPos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectObjectNoTarget(ScriptAccessory accessory, ulong owner, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectPosNoTarget(ScriptAccessory accessory, Vector3 pos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null, DrawModeEnum drawMode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = pos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        accessory.Method.SendDraw(drawMode, DrawTypeEnum.Rect, dp);
    }
    public static void DrawRectObjectTarget(ScriptAccessory accessory, ulong owner, ulong target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.TargetObject = target;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawFan(ScriptAccessory accessory, Vector3 position, float rotation, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Rotation = rotation;
        dp.Scale = scale;
        dp.Radian = angle * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.FixRotation = fix;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    public static void DrawFanObject(ScriptAccessory accessory, ulong owner, float rotation, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool scaleByTime = true, bool fix = false, Vector3? offset = null)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.Rotation = rotation;
        dp.Scale = scale;
        dp.Radian = angle * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.FixRotation = fix;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    public static void DrawLine(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, float width, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = startPosition;
        dp.TargetPosition = endPosition;
        dp.Scale = new Vector2(width, 1);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
    }

    public static void DrawArrow(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, float x, float y, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = startPosition;
        dp.TargetPosition = endPosition;
        dp.Scale = new Vector2(x, y);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
    }

    public static void DrawCircleObject(ScriptAccessory accessory, ulong? ob, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0)
    {
        if (ob == null) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = ob.Value;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    public static void DrawDonut(ScriptAccessory accessory, Vector3 position, Vector2 scale, Vector2 innerscale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Radian = 2 * float.Pi;
        dp.Scale = scale;
        dp.InnerScale = innerscale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Donut, dp);
    }
}


#endregion Function Set

#region Extension Methods
public static class ExtensionMethods
{
    public static float Round(this float value, float precision) => MathF.Round(value / precision) * precision;

    public static Vector3 ToDirection(this float rotation)
    {
        return new Vector3(
            MathF.Sin(rotation),
            0f,
            MathF.Cos(rotation)
        );
    }

    public static Vector3 Quantized(this Vector3 position, float gridSize = 1f)
    {
        return new Vector3(
            MathF.Round(position.X / gridSize) * gridSize,
            MathF.Round(position.Y / gridSize) * gridSize,
            MathF.Round(position.Z / gridSize) * gridSize
        );
    }

    /// <summary>
    /// Get a player's job name or abbreviation
    /// </summary>
    public static string GetPlayerJob(
        this ScriptAccessory sa,
        IPlayerCharacter? playerObject,
        bool fullName = false
    )
    {
        if (playerObject == null) return "None";
        return fullName
            ? playerObject.ClassJob.Value.Name.ToString()
            : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    /// <summary>
    /// Check if a player is a tank class
    /// </summary>
    public static bool IsTank(IPlayerCharacter? playerObject)
    {
        if (playerObject == null) return false;
        return playerObject.ClassJob.Value.Role == 1;
    }
}

public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion

#region Rising Stones Query
public static class RisingStonHelper
{
    private const string SearchAPI = "https://apiff14risingstones.web.sdo.com/api/common/search?type=6&keywords={0}&page={1}&limit=50";
    private const string PlayerInfo = "https://ff14risingstones.web.sdo.com/pc/index.html#/me/info?uuid={0}";
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task SearchRisingStone(ScriptAccessory sa, IPlayerCharacter player)
    {
        if (player == null) return;

        var playerName = player.Name.ToString();
        var worldName = player.HomeWorld.Value.Name.ToString();

        sa.Log.Debug($"Searching Rising Stones: {playerName}@{worldName}");

        try
        {
            var page = 1;
            var isFound = false;
            const int delayBetweenRequests = 1000;

            while (!isFound && page <= 10)
            {
                var url = string.Format(SearchAPI, playerName, page);
                var response = await httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<RSPlayerSearchResult>(response);

                if (result?.data == null || result.data.Count == 0)
                {
                    sa.Log.Debug($"Rising Stones: Player {playerName} not found");
                    break;
                }

                foreach (var rsPlayer in result.data)
                {
                    if (rsPlayer.character_name == playerName && rsPlayer.group_name == worldName)
                    {
                        var uuid = rsPlayer.uuid;
                        var profileUrl = string.Format(PlayerInfo, uuid);
                        sa.Log.Debug($"Found Rising Stones profile: {profileUrl}");

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = profileUrl,
                            UseShellExecute = true
                        });

                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    await Task.Delay(delayBetweenRequests);
                    page++;
                }
            }
        }
        catch (Exception ex)
        {
            sa.Log.Error($"Failed to query Rising Stones: {ex.Message}");
        }
    }

    // JSON
    public class RSPlayerSearchResult
    {
        public int status { get; set; }
        public string message { get; set; } = "";
        public List<RSPlayer> data { get; set; } = new();
    }

    public class RSPlayer
    {
        public string uuid { get; set; } = "";
        public string character_name { get; set; } = "";
        public string group_name { get; set; } = "";
        public int level { get; set; }
    }
}
#endregion

#region FFLogs
public static class FFLogsHelper
{
    private const string FFLogsUrl = "https://cn.fflogs.com/character/{0}/{1}/{2}";

    public static void OpenFFLogs(ScriptAccessory sa, IPlayerCharacter player)
    {
        if (player == null) return;

        try
        {
            var playerName = player.Name.ToString();
            var worldName = player.HomeWorld.Value.Name.ToString();

            var dataCenterRow = player.HomeWorld.Value.DataCenter.RowId;
            var region = player.HomeWorld.Value.DataCenter.Value.Region;
            var regionAbbr = RegionToFFLogsAbbr(region);

            var url = string.Format(FFLogsUrl, regionAbbr, worldName, playerName);

            sa.Log.Debug($"Opening FFLogs: {url} (Region: {region}, DC: {dataCenterRow})");

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            sa.Log.Error($"Failed to open FFLogs: {ex.Message}");
        }
    }

    private static string RegionToFFLogsAbbr(uint region) =>
        region switch
        {
            1 => "JP",
            2 => "NA",
            3 => "EU",
            4 => "OC",
            5 => "CN",
            6 => "KR",
            _ => "CN"
        };
}
#endregion
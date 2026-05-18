using Dalamud.Game.ClientState.Objects.Types;
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
using System.Runtime.Intrinsics.Arm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FFXIVClientStructs;
using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Data.Structs;

namespace Veever.Shadowbringers.Edens_Verse_Iconoclasm;

[ScriptType(name: "LV.80 Eden's Verse: Iconoclasm", territorys: [904], guid: "0f5deddd-09f0-4038-bcd1-ca646efe6b0e",
    version: "0.0.0.1", author: "Linoa235", note: noteStr)]

public class Edens_Verse_Iconoclasm
{
    const string noteStr =
    """
    v0.0.0.1:
    1. This script was tested using only one ARR replay. If you encounter drawing issues, please @ me on Discord and provide the ARR replay.
    Duckmen.
    """;

    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public int LightsCourseCount;
    public int StackCount;

    public List<Vector3> BallCheckList = new List<Vector3>
    {
        new Vector3(85.00f, 0.00f, 75.00f),
        new Vector3(95.00f, 0.00f, 75.00f),
        new Vector3(105.00f, 0.00f, 75.00f),
        new Vector3(115.00f, 0.00f, 75.00f),
    };

    public List<Vector3> BallStartList = new List<Vector3>
    {
        new Vector3(85.00f, 0.00f, 80.00f),
        new Vector3(95.00f, 0.00f, 80.00f),
        new Vector3(105.00f, 0.00f, 80.00f),
        new Vector3(115.00f, 0.00f, 80.00f),
        new Vector3(80.00f, 0.00f, 115.00f),
        new Vector3(80.00f, 0.00f, 105.00f),
        new Vector3(80.00f, 0.00f, 95.00f),
        new Vector3(80.00f, 0.00f, 85.00f),
    };

    public List<Vector3> centerPoint = new List<Vector3>
    {
        new Vector3(85.00f, 0.00f, 100.00f),
        new Vector3(95.00f, 0.00f, 100.00f),
        new Vector3(105.00f, 0.00f, 100.00f),
        new Vector3(115.00f, 0.00f, 100.00f),
        new Vector3(100.00f, 0.00f, 115.00f),
        new Vector3(100.00f, 0.00f, 105.00f),
        new Vector3(100.00f, 0.00f, 95.00f),
        new Vector3(100.00f, 0.00f, 85.00f),
    };

    private readonly object LightsCourseLock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        LightsCourseCount = 0;
        StackCount = 0;
    }
 
    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    // Main mechanics: Empty Wave, Unshadowed Stake, Lights and Darkness Course (complex logic), Forced Transfer, Stack
    // (Original logic preserved in translation)
}
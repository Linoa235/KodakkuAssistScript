using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using System.Reflection.Metadata;
using System.Net;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.Intrinsics.Arm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Veever.Other.Cliffhanger;

[ScriptType(name: "Gold Saucer - Cliffhanger", territorys: [144], guid: "4359482a-b7a8-4eb1-9a42-ffa3c79d6b7b",
    version: "0.0.0.3", author: "Veever", note: noteStr)]

public class Cliffhanger
{
    const string noteStr =
    """
    v0.0.0.3:
    1. Supports classic version of Cliffhanger
    2. Bomb explosion range and timing are precise, refer to the drawing
    3. TODO: Another version of Cliffhanger (will do when encountered)
    4. Gold Saucer!! Saucer!!!!!!!
    Duckmen.
    """;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        accessory.Method
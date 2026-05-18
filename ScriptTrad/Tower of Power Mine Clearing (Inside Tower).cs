using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Dalamud.Interface.ManagedFontAtlas;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Extensions;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameOperate;
using System.Collections.Concurrent;
using KodakkuAssist.Module.Draw.Manager;

namespace KodakkuAssistXSZYYS
{
    internal static class EOMineDatabase
    {
        public struct Mine
        {
            public Vector3 Position;
            public bool IsLarge;
        }

        public class MineGroup
        {
            public List<Mine> Mines = new List<Mine>();
        }

        public static readonly Dictionary<uint, List<MineGroup>> MinesByMap = new Dictionary<uint, List<MineGroup>>
        {
            [969] = new List<MineGroup> { /* ... coordinates ... */ },
            [970] = new List<MineGroup> { /* ... coordinates ... */ },
            [971] = new List<MineGroup> { /* ... coordinates ... */ },
            [986] = new List<MineGroup> { /* ... coordinates ... */ },
        };
    }

    [ScriptType(
        name: "Tower of Power Mine Clearing (Inside Tower)",
        guid: "e39a74e5-1bc3-430c-a15c-64cbc2ce5cb9",
        territorys: [1252],
        version: "0.1.1",
        Author: "Linoa235",
        note: "Type [/e Crescent Mine Clear] in chat to start mine clearing. Type again to hide. Display lasts 1800s. If display disappears, re-enter the command.\nMajor updates:\n- Added auto show/hide mine markers on map change.\n- Data restructured by Map ID, ensuring only current map mines are displayed."
    )]
    public class TowerOfPowerMineClearing
    {
        private bool _areMinesShown = false;
        private uint _currentMapId = 0;
        private readonly object _lock = new object();

        public void Init(ScriptAccessory accessory)
        {
            lock (_lock)
            {
                _areMinesShown = false;
                _currentMapId = 0;
            }
            accessory.Method.RemoveDraw(".*");
            accessory.Log.Debug("Tower of Power Mine Clearing Script initialized.");
        }

        [ScriptMethod(
            name: "Toggle Mine Location Display",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:Echo", "Message:Crescent Mine Clear"]
        )]
        public void ToggleMineDisplay(Event @event, ScriptAccessory accessory)
        {
            lock (_lock)
            {
                if (!EOMineDatabase.MinesByMap.ContainsKey(_currentMapId))
                {
                    accessory.Method.TextInfo("No mine data for current map.", 2000);
                    return;
                }

                _areMinesShown = !_areMinesShown;

                if (_areMinesShown)
                {
                    DrawMinesForMap(accessory, _currentMapId);
                    accessory.Method.TextInfo("Showing mine locations", 2000);
                }
                else
                {
                    accessory.Method.RemoveDraw("EO_Mine_.*");
                    accessory.Method.TextInfo("Hiding mine locations", 2000);
                }
            }
        }

        #region Map Change Handlers
        [ScriptMethod(name: "Enter Map 969", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:969"], userControl: false)]
        public void OnEnterMap969(Event @event, ScriptAccessory accessory) => HandleEnterMineMap(969, accessory);

        [ScriptMethod(name: "Enter Map 970", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:970"], userControl: false)]
        public void OnEnterMap970(Event @event, ScriptAccessory accessory) => HandleEnterMineMap(970, accessory);

        [ScriptMethod(name: "Enter Map 971", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:971"], userControl: false)]
        public void OnEnterMap971(Event @event, ScriptAccessory accessory) => HandleEnterMineMap(971, accessory);

        [ScriptMethod(name: "Enter Map 986", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:986"], userControl: false)]
        public void OnEnterMap986(Event @event, ScriptAccessory accessory) => HandleEnterMineMap(986, accessory);

        private async void HandleEnterMineMap(uint mapId, ScriptAccessory accessory)
        {
            if (mapId == _currentMapId)
            {
                accessory.Log.Debug($"Duplicate map entry event for {mapId}, ignored.");
                return;
            }
            uint newMapId;
            lock (_lock)
            {
                _currentMapId = mapId;
                newMapId = _currentMapId;
                accessory.Method.RemoveDraw("EO_.*");
            }
            await Task.Delay(50);
            lock (_lock)
            {
                if (_currentMapId != newMapId) return;
                _areMinesShown = true;
                DrawMinesForMap(accessory, newMapId);
                accessory.Method.TextInfo($"Entered mine area ({newMapId}), markers auto-shown.", 3000);
            }
        }
        #endregion

        [ScriptMethod(name: "Large Mine Spawn Handling", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2014585"])]
        public void OnLargeMineSpawn(Event @event, ScriptAccessory accessory) => HandleMineSpawn(@event.SourcePosition, true, accessory);

        [ScriptMethod(name: "Small Mine Spawn Handling", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2014584"])]
        public void OnSmallMineSpawn(Event @event, ScriptAccessory accessory) => HandleMineSpawn(@event.SourcePosition, false, accessory);

        private async void HandleMineSpawn(Vector3 spawnedPosition, bool isLargeSpawned, ScriptAccessory accessory)
        {
            if (!EOMineDatabase.MinesByMap.TryGetValue(_currentMapId, out var currentMapMines)) return;

            int groupIndex = 0;
            foreach (var group in currentMapMines)
            {
                int mineIndex = 0;
                foreach (var mine in group.Mines)
                {
                    if (Vector3.Distance(mine.Position, spawnedPosition) < 1.5f)
                    {
                        int innerMineIndex = 0;
                        foreach (var mineToClear in group.Mines)
                        {
                            accessory.Method.RemoveDraw($"EO_Mine_G{groupIndex}_M{innerMineIndex}");
                            innerMineIndex++;
                        }

                        await Task.Delay(50);

                        DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"EO_Explosion_G{groupIndex}_M{mineIndex}";
                        dp.Position = spawnedPosition;
                        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 0.6f);
                        dp.DestoryAt = 1000000;
                        dp.Scale = isLargeSpawned ? new Vector2(30f, 30f) : new Vector2(7f, 7f);
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        return;
                    }
                    mineIndex++;
                }
                groupIndex++;
            }
        }

        private void DrawMinesForMap(ScriptAccessory accessory, uint mapId)
        {
            if (!EOMineDatabase.MinesByMap.TryGetValue(mapId, out var mineGroups)) return;

            const long displayDuration = 1800000;
            var smallMineColor = new Vector4(1.0f, 0.65f, 0.0f, 2.0f);
            var largeMineColor = new Vector4(0.86f, 0.08f, 0.23f, 2.0f);
            var mineRadius = new Vector2(4f, 4f);

            int groupIndex = 0;
            foreach (var group in mineGroups)
            {
                int mineIndex = 0;
                foreach (var mine in group.Mines)
                {
                    DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"EO_Mine_G{groupIndex}_M{mineIndex}";
                    dp.Position = mine.Position;
                    dp.DestoryAt = displayDuration;
                    dp.Color = mine.IsLarge ? largeMineColor : smallMineColor;
                    dp.Scale = mineRadius;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    mineIndex++;
                }
                groupIndex++;
            }
        }

        [ScriptMethod(name: "Thief Mine Scan", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41648"])]
        public void OnThiefScan(Event @event, ScriptAccessory accessory)
        {
            if (!EOMineDatabase.MinesByMap.TryGetValue(_currentMapId, out var currentMapMines)) return;

            var scanPosition = @event.SourcePosition;
            const float scanRadius = 15f;

            int groupIndex = 0;
            foreach (var group in currentMapMines)
            {
                int mineIndex = 0;
                foreach (var mine in group.Mines)
                {
                    if (Vector3.Distance(mine.Position, scanPosition) <= scanRadius)
                    {
                        accessory.Method.RemoveDraw($"EO_Mine_G{groupIndex}_M{mineIndex}");
                    }
                    mineIndex++;
                }
                groupIndex++;
            }
        }

        [ScriptMethod(name: "Hunter Mine Scan", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41601"])]
        public void OnHunterScan(Event @event, ScriptAccessory accessory)
        {
            if (!EOMineDatabase.MinesByMap.TryGetValue(_currentMapId, out var currentMapMines)) return;

            var scanPosition = @event.EffectPosition;
            const float scanRadius = 9f;

            int groupIndex = 0;
            foreach (var group in currentMapMines)
            {
                int mineIndex = 0;
                foreach (var mine in group.Mines)
                {
                    if (Vector3.Distance(mine.Position, scanPosition) <= scanRadius)
                    {
                        accessory.Method.RemoveDraw($"EO_Mine_G{groupIndex}_M{mineIndex}");
                    }
                    mineIndex++;
                }
                groupIndex++;
            }
        }

        [ScriptMethod(name: "Mine Explosion", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(42050|42051)$"])]
        public void OnMineExplosion(Event @event, ScriptAccessory accessory)
        {
            if (!EOMineDatabase.MinesByMap.TryGetValue(_currentMapId, out var currentMapMines)) return;

            var explosionPosition = @event.SourcePosition;

            int groupIndex = 0;
            foreach (var group in currentMapMines)
            {
                int mineIndex = 0;
                foreach (var mine in group.Mines)
                {
                    if (Vector3.Distance(mine.Position, explosionPosition) < 1.5f)
                    {
                        accessory.Method.RemoveDraw($"EO_Mine_G{groupIndex}_M{mineIndex}");
                        accessory.Method.RemoveDraw($"EO_Explosion_G{groupIndex}_M{mineIndex}");
                        return;
                    }
                    mineIndex++;
                }
                groupIndex++;
            }
        }
    }
}
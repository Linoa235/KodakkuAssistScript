using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Extensions;

namespace KodakkuAssistXSZYYSPolice;

[ScriptType(
    name: "Tower of Power (Police, guid: "17b36a42-c70e-454d-9c85-43709372eed3")",
    territorys: [1252],
    $1188c3e02-3adc-4c81-96da-ee932c70b17f",
    version: "0.0.2",
    Author: "Linoa235",
    note: "Police functions for Tower of Power instance, providing key mechanic marker announcements and checks. Also supports active checks for money throwing, resurrection, food, blue potions, etc.\n\n------------ Echo channel query commands ------------\nCheck Blue Potions: Type [/e blue potion check] to see alchemist blue potion usage, [/e blue potion clear] to clear data\nCheck Resurrection: Type [/e resurrection check <numbers>...], e.g., [/e resurrection check 1 2] to show players with 1 and 2 resurrection remaining. Without numbers, shows all players with 0-3 resurrections\nCheck Food: Type [/e food check] to show players without food or with remaining time below threshold\nCheck Money Throw: Type [/e money throw check] to show players who used money throw and their counts, [/e money throw clear] to clear statistics"
)]
public class TowerPolice
{
    [UserSetting("Police Mode (outputs key mechanic markers to echo channel)")]
    public bool PoliceMode { get; set; } = false;

    [UserSetting("Receive party check requests for money/resurrection/food/blue potions")]
    public bool ReceivePartyCheckRequest { get; set; } = false;

    [UserSetting("Food remaining time threshold (minutes), only outputs players with remaining time <= this value")]
    public int FoodRemainingTimeThreshold { get; set; } = 0;

    [UserSetting("Blue Potion Check Scope (party only)")]
    public bool Partycheck { get; set; } = false;

    [UserSetting("Developer Mode (debug logs)")]
    public bool EnableDeveloperMode { get; set; } = false;

    private static readonly Dictionary<uint, string> _supportJobStatus = new()
    {
        { 4242, "Support Freelancer" },
        { 4358, "Support Knight" },
        { 4359, "Support Berserker" },
        { 4360, "Support Monk" },
        { 4361, "Support Hunter" },
        { 4362, "Support Samurai" },
        { 4363, "Support Bard" },
        { 4364, "Support Geomancer" },
        { 4365, "Support Time Mage" },
        { 4366, "Support Cannoneer" },
        { 4367, "Support Chemist" },
        { 4368, "Support Oracle" },
        { 4369, "Support Thief" }
    };

    private string GetSupportJob(IPlayerCharacter player)
    {
        if (player == null) return "None";
        var status = player.StatusList.FirstOrDefault(s => _supportJobStatus.ContainsKey(s.StatusId));
        return status != null ? _supportJobStatus[status.StatusId] : "None";
    }

    private readonly Dictionary<string, Dictionary<string, int>> _moneyThrowCounts = new();
    private readonly object _moneyThrowLock = new();
    private readonly Dictionary<string, Dictionary<string, int>> _bluePotionCounts = new();
    private readonly object _bluePotionLock = new();
    private readonly HashSet<ulong> _checkedPreyPlayers = new();
    private readonly object _preyCheckLock = new();
    private readonly HashSet<ulong> _sacredBowPreyRecordedPlayers = new();
    private readonly object _sacredBowPreyLock = new();
    private readonly Dictionary<int, List<(ulong PlayerId, float Duration)>> _lanceShareAssignments = new();
    private readonly object _lanceShareLock = new();

    private static readonly List<Vector3> SquarePositions = new()
    {
        new Vector3(100, 0, 60), new Vector3(140, 0, 60), new Vector3(140, 0, 100),
        new Vector3(140, 0, 140), new Vector3(100, 0, 140), new Vector3(60, 0, 140),
        new Vector3(60, 0, 100), new Vector3(60, 0, 60)
    };

    private static readonly float[] SquareAngles = new float[]
    {
        MathF.PI / 4, MathF.PI / 2, 3 * MathF.PI / 4, MathF.PI,
        -3 * MathF.PI / 4, -MathF.PI / 2, -MathF.PI / 4, 0
    };

    private const float PREY_DURATION_9S = 9.0f;
    private const float PREY_DURATION_13S = 13.0f;
    private const float PREY_DURATION_21S = 21.0f;
    private const float PREY_DURATION_TOLERANCE = 0.05f;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");

        lock (_moneyThrowLock) { _moneyThrowCounts.Clear(); }
        lock (_bluePotionLock) { _bluePotionCounts.Clear(); }
        lock (_preyCheckLock) { _checkedPreyPlayers.Clear(); }
        lock (_sacredBowPreyLock) { _sacredBowPreyRecordedPlayers.Clear(); }
        lock (_lanceShareLock) { _lanceShareAssignments.Clear(); }
    }

    #region Boss 1: Stack Marker Announcements
    [ScriptMethod(name: "Stack Marker Announcement (South)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40623"], userControl: false)]
    public void OnSouthStack(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        var target = accessory.Data.Objects.SearchById(@event.TargetId);
        if (target != null) accessory.Method.SendChat($"/e Stack (South) marker: {target.Name}");
    }

    [ScriptMethod(name: "Stack Marker Announcement (North)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40622"], userControl: false)]
    public void OnNorthStack(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        var target = accessory.Data.Objects.SearchById(@event.TargetId);
        if (target != null) accessory.Method.SendChat($"/e Stack (North) marker: {target.Name}");
    }
    #endregion

    #region Boss 1: Meteor Marker Announcement
    [ScriptMethod(name: "Meteor Marker Announcement", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4339"], userControl: false)]
    public void OnCometeorStatusAdd(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        var target = accessory.Data.Objects.SearchById(@event.TargetId);
        if (target != null) accessory.Method.SendChat($"/e Meteor marker: {target.Name}");
    }
    #endregion

    #region Boss 2: Snowball Tether Marker Announcement
    [ScriptMethod(name: "Snowball Tether Marker Announcement", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0112"], userControl: false)]
    public void OnGlacialImpactTether(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        var target = accessory.Data.Objects.SearchById(@event.TargetId);
        if (target != null) accessory.Method.SendChat($"/e Snowball tether marker: {target.Name}");
    }
    #endregion

    #region Final Boss: Great Axe Prey Announcements
    [ScriptMethod(name: "Great Axe Prey Announcement (9s)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4351"], userControl: false)]
    public void GreatAxePrey(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        if (float.TryParse(@event["Duration"], out var duration) && Math.Abs(duration - PREY_DURATION_9S) < PREY_DURATION_TOLERANCE)
        {
            var target = accessory.Data.Objects.SearchById(@event.TargetId);
            if (target != null) accessory.Method.SendChat($"/e Large circle (9s) marker: {target.Name}");
        }
    }

    [ScriptMethod(name: "Great Axe Prey Announcement (21s)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4352"], userControl: false)]
    public void GreatAxePreyLong(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        CheckPreyPosition(accessory, @event.TargetId);
        if (float.TryParse(@event["Duration"], out var duration) && Math.Abs(duration - PREY_DURATION_21S) < PREY_DURATION_TOLERANCE)
        {
            var target = accessory.Data.Objects.SearchById(@event.TargetId);
            if (target != null) accessory.Method.SendChat($"/e Large circle (21s) marker: {target.Name}");
        }
    }
    #endregion

    #region Final Boss: Small Axe Prey Announcement
    [ScriptMethod(name: "Small Axe Prey Announcement", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4350"], userControl: false)]
    public void LesserAxePrey(Event @event, ScriptAccessory accessory)
    {
        if (!PoliceMode) return;
        CheckPreyPosition(accessory, @event.TargetId);
        if (float.TryParse(@event["Duration"], out var duration))
        {
            if (Math.Abs(duration - PREY_DURATION_13S) < PREY_DURATION_TOLERANCE)
            {
                var target = accessory.Data.Objects.SearchById(@event.TargetId);
                if (target != null) accessory.Method.SendChat($"/e Small circle (13s) marker: {target.Name}");
            }
            else if (Math.Abs(duration - PREY_DURATION_21S) < PREY_DURATION_TOLERANCE)
            {
                var target = accessory.Data.Objects.SearchById(@event.TargetId);
                if (target != null) accessory.Method.SendChat($"/e Small circle (21s) marker: {target.Name}");
            }
        }
    }
    #endregion

    #region Final Boss: Sacred Bow (Akh Morn) Announcement and Edge Check
    [ScriptMethod(name: "Sacred Bow Marker Record and Broadcast", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4338"])]
    public void SacredBowPrey_RecordAndBroadcast(Event @event, ScriptAccessory accessory)
    {
        var player = accessory.Data.Objects.SearchById(@event.TargetId);
        if (player == null) return;

        lock (_sacredBowPreyLock)
        {
            if (_sacredBowPreyRecordedPlayers.Contains(player.EntityId)) return;
            _sacredBowPreyRecordedPlayers.Add(player.EntityId);

            if (float.TryParse(@event["Duration"], out var duration))
            {
                lock (_lanceShareLock)
                {
                    bool alreadyRecorded = _lanceShareAssignments.Values.Any(list => list.Any(p => p.PlayerId == player.EntityId));
                    if (!alreadyRecorded)
                    {
                        int platformIndex = -1;
                        for (int i = 0; i < SquarePositions.Count; i++)
                        {
                            if (IsPointInRotatedRect(player.Position, SquarePositions[i], 20, 20, SquareAngles[i]))
                            {
                                platformIndex = i % 3;
                                break;
                            }
                        }
                        if (platformIndex != -1)
                        {
                            if (!_lanceShareAssignments.ContainsKey(platformIndex))
                                _lanceShareAssignments[platformIndex] = new List<(ulong, float)>();
                            _lanceShareAssignments[platformIndex].Add((player.EntityId, duration));
                            if (EnableDeveloperMode)
                                accessory.Log.Debug($"Sacred Bow recorded: {player.Name.TextValue} on platform {platformIndex + 1}, duration {duration:F2}s");
                        }
                        else if (EnableDeveloperMode)
                        {
                            accessory.Log.Debug($"Sacred Bow recorded: {player.Name.TextValue} is offender, not on any platform.");
                        }
                    }
                }

                if (PoliceMode)
                {
                    int reportPlatformIndex = -1;
                    for (int i = 0; i < SquarePositions.Count; i++)
                    {
                        if (IsPointInRotatedRect(player.Position, SquarePositions[i], 20, 20, SquareAngles[i]))
                        {
                            reportPlatformIndex = i % 3;
                            break;
                        }
                    }
                    string platformName = reportPlatformIndex switch
                    {
                        0 => "Bottom",
                        1 => "Top Right",
                        2 => "Top Left",
                        _ => "Offender"
                    };
                    accessory.Method.SendChat($"/e Sacred Bow marker: {player.Name.TextValue} - {platformName} ({duration:F1}s)");
                }
            }
        }
    }

    [ScriptMethod(name: "Sacred Bow Edge Check", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4338"])]
    public void SacredBowPrey_CheckOnExpire(Event @event, ScriptAccessory accessory)
    {
        if (!float.TryParse(@event["Duration"], out var remainingDuration) || remainingDuration > 0.1f) return;

        var player = accessory.Data.Objects.SearchById(@event.TargetId);
        if (player == null || player.IsDead) return;

        lock (_lanceShareLock)
        {
            int initialPlatform = -1;
            (ulong PlayerId, float Duration) assignment = (0, 0);
            foreach (var entry in _lanceShareAssignments)
            {
                var found = entry.Value.FirstOrDefault(p => p.PlayerId == player.EntityId);
                if (found.PlayerId != 0)
                {
                    initialPlatform = entry.Key;
                    assignment = found;
                    break;
                }
            }
            if (initialPlatform == -1) return;

            var sortedPlayers = _lanceShareAssignments[initialPlatform].OrderBy(p => p.Duration).ToList();
            int orderIndex = sortedPlayers.FindIndex(p => p.PlayerId == player.EntityId);

            bool shouldCheck = false;
            switch (initialPlatform)
            {
                case 0: if (orderIndex == 0 || orderIndex == 2) shouldCheck = true; break;
                case 1: if (orderIndex == 1 || orderIndex == 2) shouldCheck = true; break;
                case 2: if (orderIndex == 0 || orderIndex == 1) shouldCheck = true; break;
            }

            if (shouldCheck)
            {
                if (!IsCircleFullyContainedInAnyPlatform(player.Position))
                {
                    if (PoliceMode) accessory.Method.SendChat($"/e Stack edge: {player.Name.TextValue}");
                }
            }
        }
    }
    #endregion

    #region Position Check Helper Methods
    private void CheckPreyPosition(ScriptAccessory accessory, ulong targetId)
    {
        lock (_preyCheckLock)
        {
            if (_checkedPreyPlayers.Contains(targetId)) return;
            _checkedPreyPlayers.Add(targetId);
        }
        if (!PoliceMode) return;

        var player = accessory.Data.Objects.SearchById(targetId);
        if (player == null) return;

        bool isInAnySquare = false;
        for (int i = 0; i < SquarePositions.Count; i++)
        {
            if (IsPointInRotatedRect(player.Position, SquarePositions[i], 20, 20, SquareAngles[i]))
            {
                isInAnySquare = true;
                break;
            }
        }

        if (!isInAnySquare) accessory.Method.SendChat($"/e {player.Name} wrong position!");
    }

    private bool IsPointInRotatedRect(Vector3 point, Vector3 rectCenter, float rectWidth, float rectHeight, float rectAngleRad)
    {
        float translatedX = point.X - rectCenter.X;
        float translatedZ = point.Z - rectCenter.Z;
        float cosAngle = MathF.Cos(-rectAngleRad);
        float sinAngle = MathF.Sin(-rectAngleRad);
        float rotatedX = translatedX * cosAngle - translatedZ * sinAngle;
        float rotatedZ = translatedX * sinAngle + translatedZ * cosAngle;
        return (Math.Abs(rotatedX) <= rectWidth / 2) && (Math.Abs(rotatedZ) <= rectHeight / 2);
    }

    private bool IsCircleFullyContainedInAnyPlatform(Vector3 circleCenter)
    {
        const float circleRadius = 6f;
        const float platformSize = 20f;
        float shrunkenSize = platformSize - 2 * circleRadius;

        for (int i = 0; i < SquarePositions.Count; i++)
        {
            if (IsPointInRotatedRect(circleCenter, SquarePositions[i], shrunkenSize, shrunkenSize, SquareAngles[i]))
                return true;
        }
        return false;
    }
    #endregion

    #region Resurrection Check Function
    [ScriptMethod(name: "Check Resurrection", eventType: EventTypeEnum.Chat, eventCondition: ["Type:regex:^(Echo|Party)$"])]
    public async void CheckResurrection(Event @event, ScriptAccessory accessory)
    {
        try
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            string message = @event["Message"];
            if (!message.StartsWith("resurrection check")) return;
            string[] parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<int> targetCounts = new();
            if (parts.Length > 1)
            {
                foreach (var part in parts.Skip(1))
                    if (int.TryParse(part, out int c)) targetCounts.Add(c);
            }
            else
            {
                targetCounts.AddRange(Enumerable.Range(0, 4));
            }

            var allResurrectionData = new List<Tuple<string, string, string, int>>();
            foreach (var gameObject in accessory.Data.Objects)
            {
                if (gameObject is IPlayerCharacter player)
                {
                    string playerName = player.Name.TextValue;
                    string classJob = player.ClassJob.Value.Name.ToString();
                    string supportJob = GetSupportJob(player);
                    int resurrectionCount = 0;
                    var resStatus = player.StatusList.FirstOrDefault(s => s.StatusId == 4262 || s.StatusId == 4263);
                    if (resStatus != null) resurrectionCount = resStatus.Param;
                    allResurrectionData.Add(new Tuple<string, string, string, int>(playerName, classJob, supportJob, resurrectionCount));
                }
            }

            accessory.Method.SendChat($"/{channel} --- Resurrection Check Start ---");
            foreach (var count in targetCounts)
            {
                await Task.Delay(200);
                await OutputResurrectionCheck(accessory, channel, allResurrectionData, count);
            }
        }
        catch (Exception ex) { accessory.Log.Error($"CheckResurrection error: {ex.Message}"); }
    }

    private static async Task OutputResurrectionCheck(ScriptAccessory accessory, string channel, List<Tuple<string, string, string, int>> allResurrectionData, int targetCount)
    {
        var filteredData = allResurrectionData.Where(t => t.Item4 == targetCount).ToList();
        if (filteredData.Count > 0)
        {
            accessory.Method.SendChat($"/{channel} --- Players with {targetCount} resurrection(s) ({filteredData.Count} players) ---");
            foreach (var data in filteredData)
            {
                await Task.Delay(100);
                accessory.Method.SendChat($"/{channel} {data.Item1} ({data.Item2} | {data.Item3})");
            }
        }
        else
        {
            accessory.Method.SendChat($"/{channel} --- No players found with {targetCount} resurrection(s) ---");
        }
    }
    #endregion

    #region Food Check Function
    [ScriptMethod(name: "Check Food", eventType: EventTypeEnum.Chat, eventCondition: ["Type:regex:^(Echo|Party)$", "Message:food check"])]
    public async void CheckFoodStatus(Event @event, ScriptAccessory accessory)
    {
        try
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            int towerPlayerCount = 0;
            var foodStatusData = new List<Tuple<string, string, string, string>>();

            foreach (var gameObject in accessory.Data.Objects)
            {
                if (gameObject is IPlayerCharacter player && player.HasStatusAny([4262, 4263]))
                {
                    towerPlayerCount++;
                    string playerName = player.Name.TextValue;
                    string classJob = player.ClassJob.Value.Name.ToString();
                    string supportJob = GetSupportJob(player);
                    var foodStatus = player.StatusList.FirstOrDefault(s => s.StatusId == 48);

                    if (foodStatus == null)
                        foodStatusData.Add(new Tuple<string, string, string, string>(playerName, classJob, supportJob, "No food"));
                    else if (foodStatus.RemainingTime <= FoodRemainingTimeThreshold * 60)
                        foodStatusData.Add(new Tuple<string, string, string, string>(playerName, classJob, supportJob, $"Food remaining less than {Math.Ceiling(foodStatus.RemainingTime / 60)} minutes"));
                }
            }

            accessory.Method.SendChat($"/{channel} --- Checking players with food remaining â‰¤ {FoodRemainingTimeThreshold} minutes ({towerPlayerCount} players in tower) ---");
            if (foodStatusData.Count > 0)
            {
                var sortedData = foodStatusData.OrderBy(t => t.Item4).ToList();
                foreach (var data in sortedData)
                {
                    await Task.Delay(100);
                    accessory.Method.SendChat($"/{channel} {data.Item1} ({data.Item2} | {data.Item3}): {data.Item4}");
                }
            }
            else
            {
                await Task.Delay(100);
                accessory.Method.SendChat($"/{channel} All players meet food requirement");
            }
        }
        catch (Exception ex) { accessory.Log.Error($"CheckFoodStatus error: {ex.Message}"); }
    }
    #endregion

    #region Money Throw Check Function
    [ScriptMethod(name: "Record Money Throw Count", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41606"], userControl: false)]
    public void RecordMoneyThrow(Event @event, ScriptAccessory accessory)
    {
        var source = accessory.Data.Objects.SearchById(@event.SourceId);
        var target = accessory.Data.Objects.SearchById(@event.TargetId);
        if (source == null || target == null || !(source is IBattleChara) || !(target is IBattleChara)) return;

        string playerName = source.Name.TextValue;
        string bossName = target.Name.TextValue;

        lock (_moneyThrowLock)
        {
            if (!_moneyThrowCounts.ContainsKey(bossName)) _moneyThrowCounts[bossName] = new Dictionary<string, int>();
            if (!_moneyThrowCounts[bossName].ContainsKey(playerName)) _moneyThrowCounts[bossName][playerName] = 0;
            _moneyThrowCounts[bossName][playerName]++;
        }
    }

    [ScriptMethod(name: "Check Money Throw", eventType: EventTypeEnum.Chat, eventCondition: ["Type:regex:^(Echo|Party)$", "Message:money throw check"])]
    public async void CheckMoneyThrow(Event @event, ScriptAccessory accessory)
    {
        try
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            Dictionary<string, List<KeyValuePair<string, int>>> sortedData;
            lock (_moneyThrowLock)
            {
                if (_moneyThrowCounts.Count == 0)
                {
                    accessory.Method.SendChat($"/{channel} No money throw data recorded.");
                    return;
                }
                sortedData = new Dictionary<string, List<KeyValuePair<string, int>>>();
                foreach (var bossEntry in _moneyThrowCounts)
                    sortedData[bossEntry.Key] = bossEntry.Value.OrderBy(kvp => kvp.Value).ToList();
            }

            foreach (var bossEntry in sortedData)
            {
                accessory.Method.SendChat($"/{channel} --- {bossEntry.Key} Money Throw Statistics ---");
                foreach (var data in bossEntry.Value)
                {
                    await Task.Delay(100);
                    accessory.Method.SendChat($"/{channel} {data.Key}: {data.Value} times");
                }
            }
        }
        catch (Exception ex) { accessory.Log.Error($"CheckMoneyThrow error: {ex.Message}"); }
    }

    [ScriptMethod(name: "Clear Money Throw Data", eventType: EventTypeEnum.Chat, eventCondition: ["Type:regex:^(Echo|Party)$", "Message:money throw clear"])]
    public void ClearMoneyThrowData(Event @event, ScriptAccessory accessory)
    {
        string channel = @event["Type"].ToLower();
        if (!ReceivePartyCheckRequest && channel == "party") return;
        lock (_moneyThrowLock) { _moneyThrowCounts.Clear(); }
        accessory.Method.SendChat($"/{channel} Money throw data cleared.");
    }
    #endregion

    #region Blue Potion Check Function
    [ScriptMethod(name: "Record Blue Potion Count", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41633"], userControl: false)]
    public void RecordBluePotion(Event @event, ScriptAccessory accessory)
    {
        var source = accessory.Data.Objects.SearchById(@event.SourceId);
        var target = accessory.Data.Objects.SearchById(@event.TargetId);
        if (source == null || target == null || !(source is IPlayerCharacter) || !(target is IPlayerCharacter)) return;

        string sourcePlayerName = source.Name.TextValue;
        string targetPlayerName = target.Name.TextValue;

        lock (_bluePotionLock)
        {
            if (!_bluePotionCounts.ContainsKey(targetPlayerName)) _bluePotionCounts[targetPlayerName] = new Dictionary<string, int>();
            if (!_bluePotionCounts[targetPlayerName].ContainsKey(sourcePlayerName)) _bluePotionCounts[targetPlayerName][sourcePlayerName] = 0;
            _bluePotionCounts[targetPlayerName][sourcePlayerName]++;
        }
    }

    [ScriptMethod(name: "Check Blue Potion", eventType: EventTypeEnum.Chat, eventCondition: ["Type:regex:^(Echo|Party)$", "Message:blue potion check"])]
    public async void CheckBluePotion(Event @event, ScriptAccessory accessory)
    {
        try
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            Dictionary<string, List<KeyValuePair<string, int>>> sortedData;
            lock (_bluePotionLock)
            {
                if (_bluePotionCounts.Count == 0)
                {
                    accessory.Method.SendChat($"/{channel} No blue potion data recorded.");
                    return;
                }
                var partyMemberNames = Partycheck
                    ? accessory.Data.PartyList.Select(id => accessory.Data.Objects.SearchById(id)?.Name.TextValue).Where(name => name != null).ToHashSet()
                    : null;

                sortedData = new Dictionary<string, List<KeyValuePair<string, int>>>();
                foreach (var bossEntry in _bluePotionCounts)
                {
                    var filteredPlayers = Partycheck
                        ? bossEntry.Value.Where(kvp => partyMemberNames.Contains(kvp.Key) && partyMemberNames.Contains(bossEntry.Key)).ToList()
                        : bossEntry.Value.ToList();
                    if (filteredPlayers.Count > 0)
                        sortedData[bossEntry.Key] = filteredPlayers.OrderBy(kvp => kvp.Value).ToList();
                }
            }

            if (sortedData.Count == 0)
            {
                accessory.Method.SendChat($"/{channel} No matching blue potion data found in current range.");
                return;
            }

            foreach (var bossEntry in sortedData)
            {
                accessory.Method.SendChat($"/{channel} --- Blue Potion Statistics for {bossEntry.Key} ---");
                foreach (var data in bossEntry.Value)
                {
                    await Task.Delay(100);
                    accessory.Method.SendChat($"/{channel} {data.Key}: {data.Value} times");
                }
            }
        }
        catch (Exception ex) { accessory.Log.Error($"CheckBluePotion error: {ex.Message}"); }
    }

    [ScriptMethod(name: "Clear Blue Potion Data", eventType: EventTypeEnum.Chat, eventCondition: ["Type:regex:^(Echo|Party)$", "Message:blue potion clear"])]
    public void ClearBluePotionData(Event @event, ScriptAccessory accessory)
    {
        string channel = @event["Type"].ToLower();
        if (!ReceivePartyCheckRequest && channel == "party") return;
        lock (_bluePotionLock) { _bluePotionCounts.Clear(); }
        accessory.Method.SendChat($"/{channel} Blue potion data cleared.");
    }
    #endregion

    #region Mark Chemists Function
    [ScriptMethod(name: "Mark Chemists", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo"])]
    public async void MarkChemists(Event @event, ScriptAccessory accessory)
    {
        try
        {
            if (@event["Message"] != "mark chemists") return;
            if (EnableDeveloperMode) accessory.Log.Debug("Detected 'mark chemists' command...");

            accessory.Method.MarkClear();
            await Task.Delay(1000);

            var markType = MarkType.Attack1;
            int chemistsFound = 0;

            foreach (var gameObject in accessory.Data.Objects)
            {
                if (gameObject is IPlayerCharacter player)
                {
                    bool isChemist = player.StatusList.Any(s => s.StatusId == 4367);
                    if (isChemist)
                    {
                        accessory.Method.Mark(player.EntityId, markType);
                        chemistsFound++;
                        if (markType < MarkType.Attack8) markType++;
                    }
                }
            }
            accessory.Method.SendChat($"/e Marked {chemistsFound} chemists.");
        }
        catch (Exception ex) { accessory.Log.Error($"MarkChemists error: {ex.Message}"); }
    }
    #endregion
}
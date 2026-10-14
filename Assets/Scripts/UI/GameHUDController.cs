using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameHUDController : MonoBehaviour
{
    private const int MaxQueuedSpawns = 20000;
    private const float HudRefreshInterval = 0.25f;
    private const float BeeSelectionRadius = 30f;

    private static VisualElement activeRoot;

    private VisualElement root;
    private Label pollenDandelion;
    private Label pollenTulip;
    private Label pollenSunflower;
    private Label nectarLabel;
    private Label waxLabel;
    private Label honeyLabel;
    private Label beeCountLabel;
    private Label buildStatusLabel;
    private Label waxPercentLabel;
    private Label honeyPercentLabel;
    private Label conversionWorkersLabel;
    private Label workerCollectorsLabel;
    private Label workerScoutsLabel;
    private Label workerDefendersLabel;
    private Label workerAttackersLabel;

    private Label debugBeeCountLabel;
    private Label debugFlowerCountLabel;
    private Label debugSearchingLabel;
    private Label debugScoutingLabel;
    private Label debugFlyingLabel;
    private Label debugCollectingLabel;
    private Label debugReturningLabel;
    private Label debugWaitingLabel;
    private Label debugQueueLabel;
    private Label fpsLabel;
    private Label selectedBeeTitle;
    private Label selectedBeeActivity;
    private Label selectedBeeTarget;
    private Label selectedBeeCargo;
    private Label selectedBeeHive;

    private IntegerField spawnBeesInput;
    private IntegerField spawnFlowersInput;
    private Button spawnBeesButton;
    private Button spawnFlowersButton;
    private Button reloadSceneButton;
    private Button buildHiveButton;
    private Button selectedHiveSchoolButton;
    private Button selectedHiveDeployButton;
    private Slider waxRatioSlider;
    private Slider selectedHiveProductionSlider;
    private Slider selectedHiveDeploySlider;

    private VisualElement selectedHivePanel;
    private VisualElement selectedBeePanel;
    private VisualElement beeSelectionMarker;
    private Label selectedHiveTitle;
    private Label selectedHiveCost;
    private Label selectedHiveProductionValue;
    private Label selectedHiveUneducated;
    private Label selectedHiveEducated;
    private Label selectedHiveDeployValue;

    private readonly Dictionary<string, VisualElement> panels = new Dictionary<string, VisualElement>();
    private readonly Dictionary<string, Button> toolbarButtons = new Dictionary<string, Button>();
    private readonly Dictionary<string, Action> toolbarCallbacks = new Dictionary<string, Action>();
    private string activePanel;

    private World gameWorld;
    private EntityManager entityManager;
    private EntityQuery playerQuery;
    private EntityQuery hiveQuery;
    private EntityQuery beeQuery;
    private EntityQuery activeFlowerQuery;
    private EntityQuery regrowingFlowerQuery;
    private EntityQuery beeSpawnerQuery;
    private EntityQuery flowerSpawnerQuery;
    private bool queriesInitialized;
    private bool isReloading;
    private float refreshTimer;
    private float fpsElapsedTime;
    private int fpsFrameCount;

    private Entity selectedHiveEntity = Entity.Null;
    private Entity selectedBeeEntity = Entity.Null;

    public static bool BuildModeActive { get; private set; }

    public static bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (activeRoot == null || activeRoot.panel == null)
            return false;

        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(activeRoot.panel, screenPosition);
        VisualElement pickedElement = activeRoot.panel.Pick(panelPosition);
        return pickedElement != null && pickedElement != activeRoot;
    }

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if (root == null) return;
        root.pickingMode = PickingMode.Ignore;
        activeRoot = root;

        BindElements();
        RegisterCallbacks();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
        DisposeWorldQueries();
        CancelBuildMode();
        if (activeRoot == root) activeRoot = null;
        root = null;
    }

    private void Update()
    {
        UpdateFps(Time.unscaledDeltaTime);
        if (!TryInitializeWorld()) return;

        HandleWorldSelection();
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f) return;

        refreshTimer = HudRefreshInterval;
        UpdateResourcesAndActivity();
        UpdateProductionDisplay();
        UpdateSelectedHivePanel();
        UpdateSelectedBeePanel();
    }

    private void UpdateFps(float unscaledDeltaTime)
    {
        if (fpsLabel == null || unscaledDeltaTime <= 0f) return;

        fpsElapsedTime += unscaledDeltaTime;
        fpsFrameCount++;
        if (fpsElapsedTime < 0.5f) return;

        SetLabel(fpsLabel, $"FPS: {Mathf.RoundToInt(fpsFrameCount / fpsElapsedTime)}");
        fpsElapsedTime = 0f;
        fpsFrameCount = 0;
    }

    private void BindElements()
    {
        pollenDandelion = root.Q<Label>("pollen-dandelion");
        pollenTulip = root.Q<Label>("pollen-tulip");
        pollenSunflower = root.Q<Label>("pollen-sunflower");
        nectarLabel = root.Q<Label>("nectar");
        waxLabel = root.Q<Label>("wax");
        honeyLabel = root.Q<Label>("honey");
        beeCountLabel = root.Q<Label>("bee-count");
        buildHiveButton = root.Q<Button>("build-hive-btn");
        buildStatusLabel = root.Q<Label>("build-status");
        waxRatioSlider = root.Q<Slider>("wax-ratio-slider");
        waxPercentLabel = root.Q<Label>("wax-percent");
        honeyPercentLabel = root.Q<Label>("honey-percent");
        conversionWorkersLabel = root.Q<Label>("conversion-workers");
        workerCollectorsLabel = root.Q<Label>("worker-collectors");
        workerScoutsLabel = root.Q<Label>("worker-scouts");
        workerDefendersLabel = root.Q<Label>("worker-defenders");
        workerAttackersLabel = root.Q<Label>("worker-attackers");

        debugBeeCountLabel = root.Q<Label>("debug-bees");
        debugFlowerCountLabel = root.Q<Label>("debug-flowers");
        debugSearchingLabel = root.Q<Label>("debug-searching");
        debugScoutingLabel = root.Q<Label>("debug-scouting");
        debugFlyingLabel = root.Q<Label>("debug-flying");
        debugCollectingLabel = root.Q<Label>("debug-collecting");
        debugReturningLabel = root.Q<Label>("debug-returning");
        debugWaitingLabel = root.Q<Label>("debug-waiting");
        debugQueueLabel = root.Q<Label>("debug-queues");
        fpsLabel = root.Q<Label>("debug-fps");
        spawnBeesInput = root.Q<IntegerField>("spawn-bees-input");
        spawnFlowersInput = root.Q<IntegerField>("spawn-flowers-input");
        spawnBeesButton = root.Q<Button>("spawn-bees-button");
        spawnFlowersButton = root.Q<Button>("spawn-flowers-button");
        reloadSceneButton = root.Q<Button>("reload-scene-button");

        selectedHivePanel = root.Q<VisualElement>("selected-hive-panel");
        selectedHiveTitle = root.Q<Label>("sh-title");
        selectedHiveCost = root.Q<Label>("sh-cost");
        selectedHiveProductionValue = root.Q<Label>("sh-slider-value");
        selectedHiveProductionSlider = root.Q<Slider>("sh-production-slider");
        selectedHiveUneducated = root.Q<Label>("sh-uneducated");
        selectedHiveEducated = root.Q<Label>("sh-educated");
        selectedHiveSchoolButton = root.Q<Button>("sh-build-school-btn");
        selectedHiveDeployValue = root.Q<Label>("sh-deploy-value");
        selectedHiveDeploySlider = root.Q<Slider>("sh-deploy-slider");
        selectedHiveDeployButton = root.Q<Button>("sh-deploy-btn");

        selectedBeePanel = root.Q<VisualElement>("selected-bee-panel");
        beeSelectionMarker = root.Q<VisualElement>("bee-selection-marker");
        selectedBeeTitle = root.Q<Label>("selected-bee-title");
        selectedBeeActivity = root.Q<Label>("selected-bee-activity");
        selectedBeeTarget = root.Q<Label>("selected-bee-target");
        selectedBeeCargo = root.Q<Label>("selected-bee-cargo");
        selectedBeeHive = root.Q<Label>("selected-bee-hive");

        panels.Clear();
        panels.Add("build", root.Q<VisualElement>("build-panel"));
        panels.Add("workers", root.Q<VisualElement>("workers-panel"));
        panels.Add("laws", root.Q<VisualElement>("laws-panel"));
        panels.Add("trade", root.Q<VisualElement>("trade-panel"));

        toolbarButtons.Clear();
        toolbarButtons.Add("build", root.Q<Button>("btn-build"));
        toolbarButtons.Add("workers", root.Q<Button>("btn-workers"));
        toolbarButtons.Add("laws", root.Q<Button>("btn-laws"));
        toolbarButtons.Add("trade", root.Q<Button>("btn-trade"));
    }

    private void RegisterCallbacks()
    {
        toolbarCallbacks.Clear();
        foreach (var entry in toolbarButtons)
        {
            if (entry.Value == null) continue;
            string panelKey = entry.Key;
            Action callback = () => TogglePanel(panelKey);
            toolbarCallbacks.Add(panelKey, callback);
            entry.Value.clicked += callback;
        }

        if (buildHiveButton != null) buildHiveButton.clicked += OnBuildHiveClicked;
        if (spawnBeesButton != null) spawnBeesButton.clicked += OnSpawnBeesClicked;
        if (spawnFlowersButton != null) spawnFlowersButton.clicked += OnSpawnFlowersClicked;
        if (reloadSceneButton != null) reloadSceneButton.clicked += OnReloadSceneClicked;
        if (waxRatioSlider != null) waxRatioSlider.RegisterValueChangedCallback(OnWaxRatioChanged);
        if (selectedHiveProductionSlider != null)
            selectedHiveProductionSlider.RegisterValueChangedCallback(OnSelectedHiveProductionChanged);
        if (selectedHiveDeploySlider != null)
            selectedHiveDeploySlider.RegisterValueChangedCallback(OnSelectedHiveDeployChanged);

        if (selectedHiveSchoolButton != null)
        {
            selectedHiveSchoolButton.SetEnabled(false);
            selectedHiveSchoolButton.text = "School (coming soon)";
        }
        if (selectedHiveDeployButton != null)
        {
            selectedHiveDeployButton.SetEnabled(false);
            selectedHiveDeployButton.text = "Extract Bees (coming soon)";
        }

        foreach (var toggle in root.Query<Toggle>(className: "law-check").ToList())
            toggle.SetEnabled(false);
    }

    private void UnregisterCallbacks()
    {
        foreach (var entry in toolbarButtons)
        {
            if (entry.Value != null && toolbarCallbacks.TryGetValue(entry.Key, out var callback))
                entry.Value.clicked -= callback;
        }
        toolbarCallbacks.Clear();

        if (buildHiveButton != null) buildHiveButton.clicked -= OnBuildHiveClicked;
        if (spawnBeesButton != null) spawnBeesButton.clicked -= OnSpawnBeesClicked;
        if (spawnFlowersButton != null) spawnFlowersButton.clicked -= OnSpawnFlowersClicked;
        if (reloadSceneButton != null) reloadSceneButton.clicked -= OnReloadSceneClicked;
        if (waxRatioSlider != null) waxRatioSlider.UnregisterValueChangedCallback(OnWaxRatioChanged);
        if (selectedHiveProductionSlider != null)
            selectedHiveProductionSlider.UnregisterValueChangedCallback(OnSelectedHiveProductionChanged);
        if (selectedHiveDeploySlider != null)
            selectedHiveDeploySlider.UnregisterValueChangedCallback(OnSelectedHiveDeployChanged);
    }

    private bool TryInitializeWorld()
    {
        var currentWorld = World.DefaultGameObjectInjectionWorld;
        if (currentWorld == null || !currentWorld.IsCreated)
        {
            SetLabel(debugQueueLabel, "Waiting for simulation...");
            return false;
        }

        if (queriesInitialized)
        {
            if (gameWorld == currentWorld) return true;
            DisposeWorldQueries();
        }

        gameWorld = currentWorld;
        entityManager = currentWorld.EntityManager;
        playerQuery = entityManager.CreateEntityQuery(typeof(PlayerData));
        hiveQuery = entityManager.CreateEntityQuery(
            typeof(HiveResources), typeof(HivePopulation), typeof(HiveProduction),
            typeof(HiveInfrastructure), typeof(TeamData), typeof(LocalTransform));
        beeQuery = entityManager.CreateEntityQuery(
            typeof(BeeTaskState), typeof(BeeCarrier), typeof(TeamData),
            typeof(HiveAssignment), typeof(LocalTransform));
        activeFlowerQuery = entityManager.CreateEntityQuery(typeof(FlowerData), typeof(LocalTransform));
        regrowingFlowerQuery = entityManager.CreateEntityQuery(typeof(GrowthData));
        beeSpawnerQuery = entityManager.CreateEntityQuery(typeof(SpawnerData));
        flowerSpawnerQuery = entityManager.CreateEntityQuery(typeof(FlowerSpawnerData));
        queriesInitialized = true;
        return true;
    }

    private void DisposeWorldQueries()
    {
        if (queriesInitialized && gameWorld != null && gameWorld.IsCreated)
        {
            playerQuery.Dispose();
            hiveQuery.Dispose();
            beeQuery.Dispose();
            activeFlowerQuery.Dispose();
            regrowingFlowerQuery.Dispose();
            beeSpawnerQuery.Dispose();
            flowerSpawnerQuery.Dispose();
        }

        queriesInitialized = false;
        gameWorld = null;
    }

    private void HandleWorldSelection()
    {
        if (BuildModeActive || Camera.main == null || Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (IsPointerOverUI(mousePosition)) return;

        Entity pickedBee = FindBeeAtScreenPosition(mousePosition);
        if (pickedBee != Entity.Null)
        {
            selectedBeeEntity = pickedBee;
            selectedHiveEntity = Entity.Null;
            Hide(selectedHivePanel);
            Show(selectedBeePanel);
            Show(beeSelectionMarker);
            return;
        }

        selectedHiveEntity = FindHiveAlongScreenRay(mousePosition);
        selectedBeeEntity = Entity.Null;
        Hide(selectedBeePanel);
        Hide(beeSelectionMarker);
        if (selectedHiveEntity == Entity.Null)
            Hide(selectedHivePanel);
        else
            Show(selectedHivePanel);
    }

    private Entity FindBeeAtScreenPosition(Vector2 mousePosition)
    {
        if (beeQuery.IsEmpty) return Entity.Null;

        using var bees = beeQuery.ToEntityArray(Allocator.Temp);
        float closestDistanceSq = BeeSelectionRadius * BeeSelectionRadius;
        Entity closestBee = Entity.Null;

        for (int i = 0; i < bees.Length; i++)
        {
            float3 worldPosition = entityManager.GetComponentData<LocalTransform>(bees[i]).Position;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f) continue;

            float distanceSq = (new Vector2(screenPosition.x, screenPosition.y) - mousePosition).sqrMagnitude;
            if (distanceSq >= closestDistanceSq) continue;
            closestDistanceSq = distanceSq;
            closestBee = bees[i];
        }

        return closestBee;
    }

    private Entity FindHiveAlongScreenRay(Vector2 mousePosition)
    {
        if (hiveQuery.IsEmpty) return Entity.Null;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        float closestDistance = 3f;
        Entity closestHive = Entity.Null;
        using var hives = hiveQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < hives.Length; i++)
        {
            float3 position = entityManager.GetComponentData<LocalTransform>(hives[i]).Position;
            float3 offset = position - (float3)ray.origin;
            float projection = math.dot(offset, (float3)ray.direction);
            if (projection < 0f) continue;

            float distance = math.distance(position, (float3)ray.origin + (float3)ray.direction * projection);
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closestHive = hives[i];
        }

        return closestHive;
    }

    private void UpdateResourcesAndActivity()
    {
        byte playerTeam = playerQuery.IsEmpty ? (byte)0 : playerQuery.GetSingleton<PlayerData>().TeamID;
        float totalNectar = 0f;
        float dandelion = 0f;
        float tulip = 0f;
        float sunflower = 0f;
        float totalWax = 0f;
        float totalHoney = 0f;
        int playerBeeCount = 0;

        using (var hives = hiveQuery.ToEntityArray(Allocator.Temp))
        {
            for (int i = 0; i < hives.Length; i++)
            {
                if (entityManager.GetComponentData<TeamData>(hives[i]).TeamID != playerTeam) continue;
                var resources = entityManager.GetComponentData<HiveResources>(hives[i]);
                totalNectar += resources.storedNectar;
                totalWax += resources.storedWax;
                totalHoney += resources.storedHoney;
                if (!entityManager.HasBuffer<PollenStorage>(hives[i])) continue;

                var pollen = entityManager.GetBuffer<PollenStorage>(hives[i]);
                for (int j = 0; j < pollen.Length; j++)
                {
                    switch (pollen[j].Type)
                    {
                        case FlowerType.Dandelion: dandelion += pollen[j].Amount; break;
                        case FlowerType.Tulip: tulip += pollen[j].Amount; break;
                        case FlowerType.Sunflower: sunflower += pollen[j].Amount; break;
                    }
                }
            }
        }

        int searching = 0;
        int scouting = 0;
        int flying = 0;
        int collecting = 0;
        int returning = 0;
        int waiting = 0;
        int defenders = 0;
        int attackers = 0;
        using (var bees = beeQuery.ToEntityArray(Allocator.Temp))
        {
            for (int i = 0; i < bees.Length; i++)
            {
                if (entityManager.GetComponentData<TeamData>(bees[i]).TeamID != playerTeam) continue;
                playerBeeCount++;
                if (entityManager.HasComponent<Defender>(bees[i])) defenders++;
                if (entityManager.HasComponent<Attacker>(bees[i])) attackers++;

                switch (entityManager.GetComponentData<BeeTaskState>(bees[i]).Activity)
                {
                    case BeeActivity.SearchingForFlower: searching++; break;
                    case BeeActivity.Scouting: scouting++; break;
                    case BeeActivity.FlyingToFlower: flying++; break;
                    case BeeActivity.Collecting: collecting++; break;
                    case BeeActivity.ReturningToHive: returning++; break;
                    case BeeActivity.WaitingForFlowers:
                    case BeeActivity.WaitingForHive: waiting++; break;
                }
            }
        }

        SetLabel(pollenDandelion, $"{dandelion:F0}");
        SetLabel(pollenTulip, $"{tulip:F0}");
        SetLabel(pollenSunflower, $"{sunflower:F0}");
        SetLabel(nectarLabel, $"{totalNectar:F0}");
        SetLabel(waxLabel, $"{totalWax:F0}");
        SetLabel(honeyLabel, $"{totalHoney:F0}");
        SetLabel(beeCountLabel, playerBeeCount.ToString());
        SetLabel(debugBeeCountLabel, $"Bees: {playerBeeCount:N0}");
        SetLabel(debugFlowerCountLabel,
            $"Flowers: {activeFlowerQuery.CalculateEntityCount():N0} ready / {regrowingFlowerQuery.CalculateEntityCount():N0} regrowing");
        SetLabel(debugSearchingLabel, $"Searching: {searching:N0}");
        SetLabel(debugScoutingLabel, $"Scouting: {scouting:N0}");
        SetLabel(debugFlyingLabel, $"Flying to flowers: {flying:N0}");
        SetLabel(debugCollectingLabel, $"Collecting: {collecting:N0}");
        SetLabel(debugReturningLabel, $"Returning to hive: {returning:N0}");
        SetLabel(debugWaitingLabel, $"Waiting: {waiting:N0}");

        SetLabel(workerCollectorsLabel, playerBeeCount.ToString("N0"));
        SetLabel(workerScoutsLabel, scouting.ToString("N0"));
        SetLabel(workerDefendersLabel, defenders.ToString("N0"));
        SetLabel(workerAttackersLabel, attackers.ToString("N0"));

        int queuedBees = beeSpawnerQuery.IsEmpty ? 0 : beeSpawnerQuery.GetSingleton<SpawnerData>().numOfEntities;
        int queuedFlowers = flowerSpawnerQuery.IsEmpty ? 0 : flowerSpawnerQuery.GetSingleton<FlowerSpawnerData>().numToSpawn;
        SetLabel(debugQueueLabel, $"Queued: {queuedBees:N0} bees, {queuedFlowers:N0} flowers");
    }

    private void UpdateProductionDisplay()
    {
        int totalConversionWorkers = 0;
        byte playerTeam = playerQuery.IsEmpty ? (byte)0 : playerQuery.GetSingleton<PlayerData>().TeamID;
        using var hives = hiveQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < hives.Length; i++)
        {
            if (entityManager.GetComponentData<TeamData>(hives[i]).TeamID != playerTeam) continue;
            totalConversionWorkers += entityManager.GetComponentData<HivePopulation>(hives[i]).conversionWorkers;
        }
        SetLabel(conversionWorkersLabel, totalConversionWorkers.ToString());
    }

    private void UpdateSelectedHivePanel()
    {
        if (selectedHiveEntity == Entity.Null || !entityManager.Exists(selectedHiveEntity) ||
            !entityManager.HasComponent<HiveProduction>(selectedHiveEntity))
        {
            selectedHiveEntity = Entity.Null;
            Hide(selectedHivePanel);
            return;
        }

        Show(selectedHivePanel);
        var production = entityManager.GetComponentData<HiveProduction>(selectedHiveEntity);
        var population = entityManager.GetComponentData<HivePopulation>(selectedHiveEntity);
        var infrastructure = entityManager.GetComponentData<HiveInfrastructure>(selectedHiveEntity);
        SetLabel(selectedHiveTitle, production.IsCapital ? "Capital Hive" : "Beehive");
        SetLabel(selectedHiveCost, production.IsCapital ? "Cost per Bee: 25 Nectar" : "Cost per Bee: 50 Nectar");
        SetLabel(selectedHiveProductionValue, $"Production: {production.TargetBeesPer5Min} Bees / 5min");
        if (selectedHiveProductionSlider != null)
            selectedHiveProductionSlider.SetValueWithoutNotify(production.TargetBeesPer5Min);
        SetLabel(selectedHiveUneducated, $"Uneducated: {population.uneducatedBees}");
        SetLabel(selectedHiveEducated, $"Educated: {population.educatedBees}");

        if (selectedHiveSchoolButton != null)
        {
            selectedHiveSchoolButton.SetEnabled(false);
            selectedHiveSchoolButton.text = infrastructure.hasSchool ? "School built" : "School (coming soon)";
        }

        if (selectedHiveDeploySlider != null)
        {
            selectedHiveDeploySlider.highValue = math.max(1, population.educatedBees);
            float deployCount = Mathf.Clamp(selectedHiveDeploySlider.value, 0, population.educatedBees);
            selectedHiveDeploySlider.SetValueWithoutNotify(deployCount);
            SetLabel(selectedHiveDeployValue, $"Extract: {Mathf.RoundToInt(deployCount)} Bees");
        }
    }

    private void UpdateSelectedBeePanel()
    {
        if (selectedBeeEntity == Entity.Null || !entityManager.Exists(selectedBeeEntity) ||
            !entityManager.HasComponent<BeeTaskState>(selectedBeeEntity))
        {
            selectedBeeEntity = Entity.Null;
            Hide(selectedBeePanel);
            Hide(beeSelectionMarker);
            return;
        }

        Show(selectedBeePanel);
        var activity = entityManager.GetComponentData<BeeTaskState>(selectedBeeEntity);
        var carrier = entityManager.GetComponentData<BeeCarrier>(selectedBeeEntity);
        var hiveAssignment = entityManager.GetComponentData<HiveAssignment>(selectedBeeEntity);
        float3 beePosition = entityManager.GetComponentData<LocalTransform>(selectedBeeEntity).Position;
        UpdateBeeSelectionMarker(beePosition);
        float pollen = 0f;
        if (entityManager.HasBuffer<PollenStorage>(selectedBeeEntity))
        {
            var buffer = entityManager.GetBuffer<PollenStorage>(selectedBeeEntity);
            for (int i = 0; i < buffer.Length; i++) pollen += buffer[i].Amount;
        }

        SetLabel(selectedBeeTitle, $"Bee #{selectedBeeEntity.Index}");
        SetLabel(selectedBeeActivity, $"Task: {GetActivityName(activity.Activity)}");
        SetLabel(selectedBeeTarget, DescribeBeeTarget(activity));
        SetLabel(selectedBeeCargo,
            $"Pollen: {pollen:F0} / {carrier.maxPollen:F0}   Nectar: {carrier.collectedNectar:F0} / {carrier.maxNectar:F0}");

        string hiveText = hiveAssignment.currentHive != Entity.Null && entityManager.Exists(hiveAssignment.currentHive)
            ? $"Hive #{hiveAssignment.currentHive.Index}"
            : "No hive assigned";
        SetLabel(selectedBeeHive, $"Home: {hiveText}");
    }

    private void UpdateBeeSelectionMarker(float3 worldPosition)
    {
        if (beeSelectionMarker == null || Camera.main == null || beeSelectionMarker.panel == null)
            return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
        {
            Hide(beeSelectionMarker);
            return;
        }

        Vector2 panelPosition = RuntimePanelUtils.CameraTransformWorldToPanel(
            beeSelectionMarker.panel, worldPosition, Camera.main);
        beeSelectionMarker.style.left = panelPosition.x;
        beeSelectionMarker.style.top = panelPosition.y;
        Show(beeSelectionMarker);
    }

    private string DescribeBeeTarget(BeeTaskState activity)
    {
        if (activity.TargetEntity != Entity.Null && entityManager.Exists(activity.TargetEntity))
        {
            string targetKind = entityManager.HasComponent<FlowerData>(activity.TargetEntity)
                ? "Flower"
                : entityManager.HasComponent<HiveResources>(activity.TargetEntity) ? "Hive" : "Entity";
            return $"Target: {targetKind} #{activity.TargetEntity.Index}";
        }

        if (activity.Activity == BeeActivity.Scouting)
            return $"Target cell: {activity.TargetPosition.x:F0}, {activity.TargetPosition.z:F0}";
        return "Target: none";
    }

    private static string GetActivityName(BeeActivity activity)
    {
        switch (activity)
        {
            case BeeActivity.SearchingForFlower: return "searching for a flower";
            case BeeActivity.Scouting: return "scouting the field";
            case BeeActivity.FlyingToFlower: return "flying to a flower";
            case BeeActivity.Collecting: return "collecting pollen";
            case BeeActivity.ReturningToHive: return "returning to the hive";
            case BeeActivity.WaitingForFlowers: return "waiting for flowers";
            case BeeActivity.WaitingForHive: return "waiting for a hive";
            default: return "unknown";
        }
    }

    private void TogglePanel(string key)
    {
        if (activePanel == key)
        {
            CloseAllPanels();
            return;
        }

        CloseAllPanels();
        if (panels.TryGetValue(key, out var panel)) Show(panel);
        if (toolbarButtons.TryGetValue(key, out var button) && button != null)
            button.AddToClassList("selected");
        activePanel = key;
    }

    private void CloseAllPanels()
    {
        foreach (var panel in panels.Values) Hide(panel);
        foreach (var button in toolbarButtons.Values)
            if (button != null) button.RemoveFromClassList("selected");

        if (activePanel == "build") CancelBuildMode();
        activePanel = null;
    }

    private void OnBuildHiveClicked()
    {
        if (BuildModeActive)
        {
            CancelBuildMode();
            return;
        }

        BuildModeActive = true;
        if (buildHiveButton != null)
        {
            buildHiveButton.AddToClassList("active");
            buildHiveButton.text = "Cancel";
        }
        SetLabel(buildStatusLabel, "Click the ground to place a hive");
    }

    public void CancelBuildMode()
    {
        BuildModeActive = false;
        if (buildHiveButton != null)
        {
            buildHiveButton.RemoveFromClassList("active");
            buildHiveButton.text = "Place";
        }
        SetLabel(buildStatusLabel, string.Empty);
    }

    private void OnSpawnBeesClicked()
    {
        if (!queriesInitialized)
        {
            SetLabel(debugQueueLabel, "Simulation is still starting");
            return;
        }
        if (beeSpawnerQuery.IsEmpty)
        {
            SetLabel(debugQueueLabel, "Bee spawner is missing from the scene");
            return;
        }

        int requested = Mathf.Clamp(spawnBeesInput != null ? spawnBeesInput.value : 1000, 1, MaxQueuedSpawns);
        Entity spawnerEntity = beeSpawnerQuery.GetSingletonEntity();
        var spawner = entityManager.GetComponentData<SpawnerData>(spawnerEntity);
        int accepted = Mathf.Min(requested, MaxQueuedSpawns - spawner.numOfEntities);
        if (accepted <= 0)
        {
            SetLabel(debugQueueLabel, "Bee spawn queue is full");
            return;
        }

        spawner.numOfEntities += accepted;
        entityManager.SetComponentData(spawnerEntity, spawner);
        SetLabel(debugQueueLabel, $"Queued {accepted:N0} bees");
    }

    private void OnSpawnFlowersClicked()
    {
        if (!queriesInitialized)
        {
            SetLabel(debugQueueLabel, "Simulation is still starting");
            return;
        }
        if (flowerSpawnerQuery.IsEmpty)
        {
            SetLabel(debugQueueLabel, "Flower spawner is missing from the scene");
            return;
        }

        int requested = Mathf.Clamp(spawnFlowersInput != null ? spawnFlowersInput.value : 1000, 1, MaxQueuedSpawns);
        Entity spawnerEntity = flowerSpawnerQuery.GetSingletonEntity();
        var spawner = entityManager.GetComponentData<FlowerSpawnerData>(spawnerEntity);
        int accepted = Mathf.Min(requested, MaxQueuedSpawns - spawner.numToSpawn);
        if (accepted <= 0)
        {
            SetLabel(debugQueueLabel, "Flower spawn queue is full");
            return;
        }

        spawner.numToSpawn += accepted;
        entityManager.SetComponentData(spawnerEntity, spawner);
        SetLabel(debugQueueLabel, $"Queued {accepted:N0} flowers");
    }

    private void OnReloadSceneClicked()
    {
        if (isReloading) return;

        var activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid()) return;

        isReloading = true;
        if (reloadSceneButton != null) reloadSceneButton.SetEnabled(false);
        CancelBuildMode();

        StartCoroutine(ReloadSceneAfterCurrentFrame(activeScene.buildIndex, activeScene.path));
    }

    private IEnumerator ReloadSceneAfterCurrentFrame(int buildIndex, string scenePath)
    {
        yield return null;

        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null && world.IsCreated)
            world.EntityManager.CompleteAllTrackedJobs();

        if (buildIndex >= 0)
            SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        else if (!string.IsNullOrEmpty(scenePath))
            SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
    }

    private void OnWaxRatioChanged(ChangeEvent<float> evt)
    {
        int waxPercent = Mathf.RoundToInt(evt.newValue * 100f);
        SetLabel(waxPercentLabel, $"Wax {waxPercent}%");
        SetLabel(honeyPercentLabel, $"Honey {100 - waxPercent}%");
        UpdateAllHivesWaxRatio(evt.newValue);
    }

    private void OnSelectedHiveProductionChanged(ChangeEvent<float> evt)
    {
        if (selectedHiveEntity == Entity.Null || !entityManager.Exists(selectedHiveEntity)) return;
        var production = entityManager.GetComponentData<HiveProduction>(selectedHiveEntity);
        production.TargetBeesPer5Min = Mathf.RoundToInt(evt.newValue);
        entityManager.SetComponentData(selectedHiveEntity, production);
        SetLabel(selectedHiveProductionValue, $"Production: {production.TargetBeesPer5Min} Bees / 5min");
    }

    private void OnSelectedHiveDeployChanged(ChangeEvent<float> evt)
    {
        SetLabel(selectedHiveDeployValue, $"Extract: {Mathf.RoundToInt(evt.newValue)} Bees");
    }

    private void UpdateAllHivesWaxRatio(float ratio)
    {
        if (!queriesInitialized) return;
        byte playerTeam = playerQuery.IsEmpty ? (byte)0 : playerQuery.GetSingleton<PlayerData>().TeamID;
        using var hives = hiveQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < hives.Length; i++)
        {
            if (entityManager.GetComponentData<TeamData>(hives[i]).TeamID != playerTeam) continue;
            var resources = entityManager.GetComponentData<HiveResources>(hives[i]);
            resources.waxRatio = ratio;
            entityManager.SetComponentData(hives[i], resources);
        }
    }

    private static void Show(VisualElement element)
    {
        if (element != null) element.RemoveFromClassList("hidden");
    }

    private static void Hide(VisualElement element)
    {
        if (element != null) element.AddToClassList("hidden");
    }

    private static void SetLabel(Label label, string value)
    {
        if (label != null) label.text = value;
    }
}

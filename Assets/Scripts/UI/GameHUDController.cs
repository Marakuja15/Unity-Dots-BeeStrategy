using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Collections;
using System.Collections.Generic;

/// <summary>
/// Main game HUD controller.
/// Toolbar buttons toggle popup panels. Only one panel open at a time.
/// Attach to a GameObject with a UIDocument component.
/// </summary>
public class GameHUDController : MonoBehaviour
{
    // ============ Resource Labels ============
    private Label pollenDandelion;
    private Label pollenTulip;
    private Label pollenSunflower;
    private Label nectarLabel;
    private Label waxLabel;
    private Label honeyLabel;
    private Label beeCountLabel;

    // ============ Build ============
    private Button buildHiveBtn;
    private Label buildStatusLabel;

    // ============ Production ============
    private Slider waxRatioSlider;
    private Label waxPercentLabel;
    private Label honeyPercentLabel;
    private Label conversionWorkersLabel;

    // ============ Panels & Toolbar ============
    private Dictionary<string, VisualElement> panels;
    private Dictionary<string, Button> toolbarButtons;
    private string activePanel = null;

    // ============ State ============
    public static bool BuildModeActive { get; private set; }
    private EntityManager entityManager;
    private bool initialized;

    void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        var root = uiDoc.rootVisualElement;

        // Resource labels
        pollenDandelion = root.Q<Label>("pollen-dandelion");
        pollenTulip = root.Q<Label>("pollen-tulip");
        pollenSunflower = root.Q<Label>("pollen-sunflower");
        nectarLabel = root.Q<Label>("nectar");
        waxLabel = root.Q<Label>("wax");
        honeyLabel = root.Q<Label>("honey");
        beeCountLabel = root.Q<Label>("bee-count");

        // Build
        buildHiveBtn = root.Q<Button>("build-hive-btn");
        buildStatusLabel = root.Q<Label>("build-status");

        // Production
        waxRatioSlider = root.Q<Slider>("wax-ratio-slider");
        waxPercentLabel = root.Q<Label>("wax-percent");
        honeyPercentLabel = root.Q<Label>("honey-percent");
        conversionWorkersLabel = root.Q<Label>("conversion-workers");

        // Setup panels
        panels = new Dictionary<string, VisualElement>
        {
            { "build",   root.Q<VisualElement>("build-panel") },
            { "workers", root.Q<VisualElement>("workers-panel") },
            { "laws",    root.Q<VisualElement>("laws-panel") },
            { "trade",   root.Q<VisualElement>("trade-panel") }
        };

        // Setup toolbar buttons
        toolbarButtons = new Dictionary<string, Button>
        {
            { "build",   root.Q<Button>("btn-build") },
            { "workers", root.Q<Button>("btn-workers") },
            { "laws",    root.Q<Button>("btn-laws") },
            { "trade",   root.Q<Button>("btn-trade") }
        };

        // Wire toolbar button events
        foreach (var kvp in toolbarButtons)
        {
            string panelKey = kvp.Key;
            kvp.Value.clicked += () => TogglePanel(panelKey);
        }

        // Wire build and slider events
        if (buildHiveBtn != null)
            buildHiveBtn.clicked += OnBuildHiveClicked;
        if (waxRatioSlider != null)
            waxRatioSlider.RegisterValueChangedCallback(OnWaxRatioChanged);

        // Disable law toggles (not yet implemented)
        var lawToggles = root.Query<Toggle>(className: "law-check").ToList();
        foreach (var toggle in lawToggles)
            toggle.SetEnabled(false);
    }

    void Update()
    {
        if (!initialized)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;
            entityManager = world.EntityManager;
            initialized = true;
        }

        UpdateResourceDisplay();
        UpdateProductionDisplay();
    }

    // =============================================
    // Panel Toggle (only one open at a time)
    // =============================================
    private void TogglePanel(string key)
    {
        if (activePanel == key)
        {
            CloseAllPanels();
            return;
        }

        CloseAllPanels();

        if (panels.TryGetValue(key, out var panel) && panel != null)
            panel.style.display = DisplayStyle.Flex;
        if (toolbarButtons.TryGetValue(key, out var btn) && btn != null)
            btn.AddToClassList("selected");

        activePanel = key;
    }

    private void CloseAllPanels()
    {
        foreach (var kvp in panels)
        {
            if (kvp.Value != null)
                kvp.Value.style.display = DisplayStyle.None;
        }
        foreach (var kvp in toolbarButtons)
        {
            if (kvp.Value != null)
                kvp.Value.RemoveFromClassList("selected");
        }

        // Deactivate build mode if build panel closes
        if (activePanel == "build" && BuildModeActive)
        {
            BuildModeActive = false;
            if (buildHiveBtn != null)
            {
                buildHiveBtn.RemoveFromClassList("active");
                buildHiveBtn.text = "Place";
            }
            SetLabel(buildStatusLabel, "");
        }

        activePanel = null;
    }

    // =============================================
    // Resource Display
    // =============================================
    private void UpdateResourceDisplay()
    {
        var playerQuery = entityManager.CreateEntityQuery(typeof(PlayerData));
        if (playerQuery.IsEmpty) return;
        var player = playerQuery.GetSingleton<PlayerData>();

        var bufferQuery = entityManager.CreateEntityQuery(typeof(TeamDataElement));
        if (bufferQuery.IsEmpty) return;
        var bufferEntity = bufferQuery.GetSingletonEntity();
        var teamsBuffer = entityManager.GetBuffer<TeamDataElement>(bufferEntity);
        if (player.TeamID >= teamsBuffer.Length) return;
        var myTeam = teamsBuffer[player.TeamID];

        SetLabel(waxLabel, $"{myTeam.storedWax:F0}");
        SetLabel(honeyLabel, $"{myTeam.storedHoney:F0}");
        SetLabel(beeCountLabel, $"{myTeam.BeeCount}");

        float totalNectar = 0f;
        float[] pollenTotals = new float[3];

        var hiveQuery = entityManager.CreateEntityQuery(typeof(HiveResources), typeof(TeamData));
        var hiveEntities = hiveQuery.ToEntityArray(Allocator.Temp);

        for (int h = 0; h < hiveEntities.Length; h++)
        {
            var team = entityManager.GetComponentData<TeamData>(hiveEntities[h]);
            if (team.TeamID != player.TeamID) continue;

            var resources = entityManager.GetComponentData<HiveResources>(hiveEntities[h]);
            totalNectar += resources.storedNectar;

            if (entityManager.HasBuffer<PollenStorage>(hiveEntities[h]))
            {
                var pollenBuffer = entityManager.GetBuffer<PollenStorage>(hiveEntities[h]);
                for (int i = 0; i < pollenBuffer.Length && i < pollenTotals.Length; i++)
                    pollenTotals[i] += pollenBuffer[i].Amount;
            }
        }
        hiveEntities.Dispose();

        SetLabel(pollenDandelion, $"{pollenTotals[0]:F0}");
        SetLabel(pollenTulip, $"{pollenTotals[1]:F0}");
        SetLabel(pollenSunflower, $"{pollenTotals[2]:F0}");
        SetLabel(nectarLabel, $"{totalNectar:F0}");
    }

    // =============================================
    // Production Display
    // =============================================
    private void UpdateProductionDisplay()
    {
        var playerQuery = entityManager.CreateEntityQuery(typeof(PlayerData));
        if (playerQuery.IsEmpty) return;
        var player = playerQuery.GetSingleton<PlayerData>();

        int totalConversionWorkers = 0;

        var hiveQuery = entityManager.CreateEntityQuery(typeof(HivePopulation), typeof(TeamData));
        var hiveEntities = hiveQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < hiveEntities.Length; i++)
        {
            var team = entityManager.GetComponentData<TeamData>(hiveEntities[i]);
            if (team.TeamID != player.TeamID) continue;

            var pop = entityManager.GetComponentData<HivePopulation>(hiveEntities[i]);
            totalConversionWorkers += pop.conversionWorkers;
        }
        hiveEntities.Dispose();

        SetLabel(conversionWorkersLabel, $"{totalConversionWorkers}");
    }

    // =============================================
    // Build Mode
    // =============================================
    public void CancelBuildMode()
    {
        if (BuildModeActive)
        {
            BuildModeActive = false;
            if (buildHiveBtn != null)
            {
                buildHiveBtn.RemoveFromClassList("active");
                buildHiveBtn.text = "Build Hive";
            }
            SetLabel(buildStatusLabel, "");
        }
    }

    private void OnBuildHiveClicked()
    {
        BuildModeActive = !BuildModeActive;

        if (buildHiveBtn != null)
        {
            if (BuildModeActive)
            {
                buildHiveBtn.AddToClassList("active");
                buildHiveBtn.text = "Cancel";
                SetLabel(buildStatusLabel, "Click on the ground to place a hive");
            }
            else
            {
                CancelBuildMode();
            }
        }
    }

    // =============================================
    // Wax / Honey Ratio
    // =============================================
    private void OnWaxRatioChanged(ChangeEvent<float> evt)
    {
        float ratio = evt.newValue;
        int waxPct = Mathf.RoundToInt(ratio * 100);

        SetLabel(waxPercentLabel, $"Wax {waxPct}%");
        SetLabel(honeyPercentLabel, $"Honey {100 - waxPct}%");
        UpdateAllHivesWaxRatio(ratio);
    }

    private void UpdateAllHivesWaxRatio(float ratio)
    {
        var playerQuery = entityManager.CreateEntityQuery(typeof(PlayerData));
        if (playerQuery.IsEmpty) return;
        var player = playerQuery.GetSingleton<PlayerData>();

        var hiveQuery = entityManager.CreateEntityQuery(typeof(HiveResources), typeof(TeamData));
        var hiveEntities = hiveQuery.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < hiveEntities.Length; i++)
        {
            var team = entityManager.GetComponentData<TeamData>(hiveEntities[i]);
            if (team.TeamID != player.TeamID) continue;

            var resources = entityManager.GetComponentData<HiveResources>(hiveEntities[i]);
            resources.waxRatio = ratio;
            entityManager.SetComponentData(hiveEntities[i], resources);
        }
        hiveEntities.Dispose();
    }

    // =============================================
    // Helpers
    // =============================================
    private void SetLabel(Label label, string text)
    {
        if (label != null) label.text = text;
    }

    void OnDisable()
    {
        BuildModeActive = false;
        if (buildHiveBtn != null)
            buildHiveBtn.clicked -= OnBuildHiveClicked;
    }
}

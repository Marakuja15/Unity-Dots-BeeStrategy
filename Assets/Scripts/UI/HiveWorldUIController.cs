using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Creates floating UI badges for each Beehive in the world.
/// Automatically handles screen projection and DOTS data reading.
/// Attach this script to the same GameObject as GameHUDController.
/// </summary>
public class HiveWorldUIController : MonoBehaviour
{
    private VisualElement worldContainer;
    private EntityManager entityManager;
    private bool initialized;
    private EntityQuery hiveQuery;
    private EntityQuery playerQuery;

    private Dictionary<Entity, VisualElement> activeBadges = new Dictionary<Entity, VisualElement>();

    void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc != null)
        {
            worldContainer = uiDoc.rootVisualElement.Q<VisualElement>("world-space-ui");
        }
    }

    void Update()
    {
        if (worldContainer == null || Camera.main == null) return;

        if (!initialized)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;
            
            entityManager = world.EntityManager;
            // Query for everything we need to draw a hive badge
            hiveQuery = entityManager.CreateEntityQuery(
                typeof(HiveResources), 
                typeof(HivePopulation), 
                typeof(TeamData), 
                typeof(LocalTransform)
            );
            playerQuery = entityManager.CreateEntityQuery(typeof(PlayerData));
            initialized = true;
        }

        if (hiveQuery.IsEmpty)
        {
            ClearAllBadges();
            return;
        }

        var hiveEntities = hiveQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        int playerTeamId = playerQuery.IsEmpty ? -1 : playerQuery.GetSingleton<PlayerData>().TeamID;

        HashSet<Entity> currentHives = new HashSet<Entity>();

        // 1. Update or Create Badges
        foreach (var entity in hiveEntities)
        {
            currentHives.Add(entity);

            if (!activeBadges.TryGetValue(entity, out VisualElement badge))
            {
                badge = CreateBadge();
                worldContainer.Add(badge);
                activeBadges[entity] = badge;
            }

            // Get ECS data
            var resources = entityManager.GetComponentData<HiveResources>(entity);
            var pop = entityManager.GetComponentData<HivePopulation>(entity);
            var team = entityManager.GetComponentData<TeamData>(entity);
            var transform = entityManager.GetComponentData<LocalTransform>(entity);

            UpdateBadgeContent(badge, resources, pop, team, playerTeamId);
            PositionBadge(badge, transform.Position);
        }

        // 2. Remove destroyed hives
        List<Entity> toRemove = new List<Entity>();
        foreach (var kvp in activeBadges)
        {
            if (!currentHives.Contains(kvp.Key))
            {
                worldContainer.Remove(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var e in toRemove) 
            activeBadges.Remove(e);

        hiveEntities.Dispose();
    }

    private VisualElement CreateBadge()
    {
        var badge = new VisualElement();
        badge.AddToClassList("hive-badge");

        var teamLabel = new Label("Team");
        teamLabel.name = "team-label";
        teamLabel.AddToClassList("hive-badge-team");

        var statsLabel = new Label("Stats");
        statsLabel.name = "stats-label";
        statsLabel.AddToClassList("hive-badge-stats");

        badge.Add(teamLabel);
        badge.Add(statsLabel);

        return badge;
    }

    private void UpdateBadgeContent(VisualElement badge, HiveResources resources, HivePopulation pop, TeamData team, int playerTeamId)
    {
        var teamLabel = badge.Q<Label>("team-label");
        var statsLabel = badge.Q<Label>("stats-label");

        bool isPlayer = (team.TeamID == playerTeamId);
        
        // Setup team banner (Green for player, Red for enemies)
        if (isPlayer)
        {
            teamLabel.text = "Our Hive";
            teamLabel.style.backgroundColor = new StyleColor(new Color32(106, 171, 80, 255)); // Green
        }
        else
        {
            teamLabel.text = $"Enemy Team {team.TeamID}";
            teamLabel.style.backgroundColor = new StyleColor(new Color32(211, 47, 47, 255)); // Red
        }

        // Setup stats (Bees on top, Resources on bottom)
        int totalBees = pop.citizens + pop.defenders + pop.conversionWorkers;
        statsLabel.text = $"Bees: {totalBees}\nNectar: {resources.storedNectar:F0} | Wax: {resources.storedWax:F0} | Honey: {resources.storedHoney:F0}";
    }

    private void PositionBadge(VisualElement badge, float3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        // Hide if behind camera
        if (screenPos.z < 0)
        {
            badge.style.display = DisplayStyle.None;
            return;
        }

        badge.style.display = DisplayStyle.Flex;

        // UI Toolkit coordinates (Y is inverted relative to Screen space)
        var panel = badge.panel;
        if (panel != null)
        {
            Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPos, Camera.main);
            badge.style.left = panelPos.x;
            badge.style.top = panelPos.y;
        }
        else 
        {
            badge.style.left = screenPos.x;
            badge.style.top = Screen.height - screenPos.y;
        }
    }

    private void ClearAllBadges()
    {
        foreach (var kvp in activeBadges)
        {
            if (kvp.Value != null && worldContainer != null)
                worldContainer.Remove(kvp.Value);
        }
        activeBadges.Clear();
    }
}

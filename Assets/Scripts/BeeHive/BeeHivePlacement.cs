using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class BeehivePlacementSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PhysicsWorldSingleton>();
        // requires configs in the future
        // np. RequireForUpdate<BuildingConfigData>();
    }

    protected override void OnUpdate()
    {
        // Only active when player clicked "Build Hive" in HUD
        if (!GameHUDController.BuildModeActive) return;
        if (Camera.main == null || UnityEngine.InputSystem.Mouse.current == null) return;

        // Czekamy na kliknięcie
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());

            RaycastInput input = new RaycastInput
            {
                Start = cameraRay.origin,
                End = cameraRay.GetPoint(1000f),
                Filter = CollisionFilter.Default
            };

            if (physicsWorld.CastRay(input, out RaycastHit hit))
            {
                if (!SystemAPI.TryGetSingletonBuffer<TeamDataElement>(out var teamsBuffer)) return;
                if (!SystemAPI.TryGetSingleton<HivePrefab>(out var hive)) return;
                if (!SystemAPI.TryGetSingleton<PlayerData>(out var player)) return;

                var playersTeam = teamsBuffer[player.TeamID];
                
                // Tymczasowo darmowe ule
                // if (playersTeam.storedWax < 100) return;
                // playersTeam.storedWax -= 100;
                // teamsBuffer[player.TeamID] = playersTeam;

                Entity newHive = EntityManager.Instantiate(hive.Value);
                EntityManager.SetComponentData(newHive, LocalTransform.FromPosition(hit.Position));
                EntityManager.AddComponentData(newHive, new TeamData { TeamID = player.TeamID });
                
                // Wyłącz tryb budowy po postawieniu ula
                var hud = UnityEngine.Object.FindObjectOfType<GameHUDController>();
                if (hud != null) hud.CancelBuildMode();
            }
        }
    }
}
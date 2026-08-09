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

        if (Camera.main == null) return;

   
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

      
        UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

   
        float3 rayStart = cameraRay.origin;
        float3 rayEnd = cameraRay.GetPoint(1000f);

        RaycastInput input = new RaycastInput
        {
            Start = rayStart,
            End = rayEnd,
            Filter = CollisionFilter.Default 
        };


        if (physicsWorld.CastRay(input, out RaycastHit hit))
        {
            float3 hitPosition = hit.Position;


            if (Input.GetMouseButtonDown(0))
            {
                if(!SystemAPI.TryGetSingletonBuffer<TeamDataElement>(out var teamsBuffer)) return;
                if(!SystemAPI.TryGetSingleton<HivePrefab>(out var hive)) return;
                var player = SystemAPI.GetSingleton<PlayerData>();
                var playersTeam = teamsBuffer[player.TeamID];
                
                if(!(playersTeam.storedWax < 100)) return;
           
 
                Entity prefab = EntityManager.Instantiate(hive.Value);
                EntityManager.SetComponentData(prefab, LocalTransform.FromPosition(hitPosition));

                EntityManager.AddComponentData(prefab, new TeamData { TeamID = player.TeamID });
              
            }
        }
    }
}
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

                // TODO: Check resurces
                // TODO: EntityManager.Instantiate(prefabUla)
                // TODO: position
                // TODO: set teamid
                
                Debug.Log($"Zbudowano ul w punkcie: {hitPosition}");
            }
        }
    }
}
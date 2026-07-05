using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]

public partial struct FlowerSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlowerSpawnerData>(); 
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if(!SystemAPI.TryGetSingletonEntity<FlowerSpawnerData>(out Entity spawnerEntity)) return;
        RefRW<FlowerSpawnerData> spawner = SystemAPI.GetComponentRW<FlowerSpawnerData>(spawnerEntity);

        if(spawner.ValueRO.numToSpawn < 1) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        for(int i = 0; i < spawner.ValueRO.numToSpawn; i++)
        {
            RaycastInput input = new RaycastInput
            {
                Start = spawner.ValueRO.spawnLeft,
                End = spawner.ValueRO.spawnRight,
                Filter = CollisionFilter.Default 
            };
            if (physicsWorld.CastRay(input, out RaycastHit hit))
            {
                Entity newFlower = ecb.Instantiate(spawner.ValueRO.prefab);
                float3 hitPosition = hit.Position;
                ecb.AddComponent(newFlower, LocalTransform.FromPosition(hitPosition));
                var FlowerData = SystemAPI.GetComponent<FlowerData>(spawner.ValueRO.prefab);
                ecb.SetComponent(newFlower, FlowerData);

                ecb.SetComponentEnabled<FlowerData>(newFlower, true);
                
            }
        }
      
    }
}
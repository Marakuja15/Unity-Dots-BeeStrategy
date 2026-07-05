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

        var random = Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime * 1000 + 1);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        for(int i = 0; i < spawner.ValueRO.numToSpawn; i++)
        {
            float3 left = spawner.ValueRO.spawnLeft;
            float3 right = spawner.ValueRO.spawnRight;
            float3 randomPoint = new float3(
                random.NextFloat(math.min(left.x, right.x), math.max(left.x, right.x)),
                math.max(left.y, right.y) + 50f,
                random.NextFloat(math.min(left.z, right.z), math.max(left.z, right.z))
            );
        
            RaycastInput input = new RaycastInput
            {
                Start = randomPoint,                     
                End = randomPoint - new float3(0, 100f, 0),     
                Filter = CollisionFilter.Default
            };
            if (physicsWorld.CastRay(input, out RaycastHit hit))
            {
        
                Entity newFlower = ecb.Instantiate(spawner.ValueRO.prefab);
                float3 hitPosition = hit.Position;
                ecb.AddComponent(newFlower, LocalTransform.FromPosition(hitPosition));
                var flowerData = SystemAPI.GetComponent<FlowerData>(spawner.ValueRO.prefab);
                var growthData = SystemAPI.GetComponent<GrowthData>(spawner.ValueRO.prefab);
                ecb.SetComponent(newFlower, flowerData);
                ecb.SetComponent(newFlower, growthData);

                ecb.SetComponentEnabled<FlowerData>(newFlower, true);
                ecb.SetComponentEnabled<GrowthData>(newFlower, false);
            }
            spawner.ValueRW.numToSpawn -= 1;
        }
      
    }
}
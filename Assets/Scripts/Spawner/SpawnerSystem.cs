using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateBefore(typeof(RemoveSystem))]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerData>(); 
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if(!SystemAPI.TryGetSingletonEntity<SpawnerData>(out Entity spawnerEntity)) return;
        RefRW<SpawnerData> spawner = SystemAPI.GetComponentRW<SpawnerData>(spawnerEntity);

        if(spawner.ValueRO.numOfEntities < 1) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int spawnPerFrame = math.min(spawner.ValueRO.numOfEntities, 1000);
        for(int i = 0; i < spawnPerFrame; i++)
        {
            float3 randomTarget = spawner.ValueRW.random.NextFloat3(
                spawner.ValueRO.min, 
                spawner.ValueRO.max
            );

            Entity newEntity = ecb.Instantiate(spawner.ValueRO.prefab);
            
            ecb.AddComponent(newEntity, LocalTransform.FromPosition(spawner.ValueRO.spawnLocation));
            var beeData = SystemAPI.GetComponent<BeeMovementData>(spawner.ValueRO.prefab);
            beeData.moveLocation = randomTarget;
            ecb.SetComponent(newEntity, beeData);
            ecb.AddComponent<PollenCollector>(newEntity); 
            ecb.AddComponent<NeedsFlowerAssignment>(newEntity);

            ecb.SetComponentEnabled<BeeMovementData>(newEntity, true);
        }
        spawner.ValueRW.numOfEntities -= spawnPerFrame;
          
        
    }
}
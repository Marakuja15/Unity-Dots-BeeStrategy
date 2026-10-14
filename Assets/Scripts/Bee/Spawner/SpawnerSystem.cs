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
        if (!SystemAPI.TryGetSingletonEntity<SpawnerData>(out Entity spawnerEntity)) return;
        var spawner = SystemAPI.GetComponentRW<SpawnerData>(spawnerEntity);

        if (spawner.ValueRO.numOfEntities < 1) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        int spawnPerFrame = math.min(spawner.ValueRO.numOfEntities, 500);
        byte teamId = SystemAPI.TryGetSingleton<PlayerData>(out var player) ? player.TeamID : (byte)0;
        var random = spawner.ValueRO.random;
        var prefabBeeData = SystemAPI.GetComponent<BeeMovementData>(spawner.ValueRO.prefab);

        Entity closestHive = Entity.Null;
        float closestHiveDistanceSq = float.MaxValue;
        foreach (var (hiveTransform, hiveTeam, hiveEntity) in
                 SystemAPI.Query<RefRO<LocalTransform>, RefRO<TeamData>>()
                     .WithAll<HiveResources>()
                     .WithEntityAccess())
        {
            if (hiveTeam.ValueRO.TeamID != teamId) continue;
            float distanceSq = math.distancesq(spawner.ValueRO.spawnLocation, hiveTransform.ValueRO.Position);
            if (distanceSq < closestHiveDistanceSq)
            {
                closestHiveDistanceSq = distanceSq;
                closestHive = hiveEntity;
            }
        }

        int spawned = 0;
        float3 spawnMin = math.min(spawner.ValueRO.min, spawner.ValueRO.max);
        float3 spawnMax = math.max(spawner.ValueRO.min, spawner.ValueRO.max);
        for (int i = 0; i < spawnPerFrame; i++)
        {
            Entity newEntity = ecb.Instantiate(spawner.ValueRO.prefab);
            float3 randomOffset = new float3(random.NextFloat(), random.NextFloat(), random.NextFloat());
            float3 spawnPosition = spawner.ValueRO.spawnLocation + math.lerp(spawnMin, spawnMax, randomOffset);
            spawnPosition.y = math.max(spawnPosition.y, spawner.ValueRO.spawnLocation.y + 0.75f);

            ecb.SetComponent(newEntity, LocalTransform.FromPosition(spawnPosition));
            var beeData = prefabBeeData;
            beeData.moveLocation = spawnPosition;
            ecb.SetComponent(newEntity, beeData);
            ecb.SetComponentEnabled<BeeMovementData>(newEntity, false);
            ecb.AddComponent(newEntity, new TeamData { TeamID = teamId });
            ecb.SetComponent(newEntity, new HiveAssignment { currentHive = closestHive });
            ecb.SetComponent(newEntity, new BeeTaskState
            {
                Activity = BeeActivity.SearchingForFlower,
                TargetEntity = Entity.Null,
                TargetPosition = float3.zero
            });
            ecb.AddComponent<PollenCollector>(newEntity);
            ecb.AddComponent<NeedsFlowerAssignment>(newEntity);
            ecb.AddComponent<Scout>(newEntity);
            ecb.AddComponent<ReturnToHive>(newEntity);
            ecb.SetComponentEnabled<ReturnToHive>(newEntity, false);
            spawned++;
        }

        spawner.ValueRW.random = random;
        spawner.ValueRW.numOfEntities -= spawned;
        if (closestHive != Entity.Null && state.EntityManager.HasComponent<HivePopulation>(closestHive))
        {
            var population = SystemAPI.GetComponentRW<HivePopulation>(closestHive);
            population.ValueRW.citizens += spawned;
        }
    }
}

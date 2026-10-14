using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[BurstCompile]
public partial struct FlowerGrowthSystem : ISystem
{
    private EntityQuery growthQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        growthQuery = SystemAPI.QueryBuilder()
            .WithAll<FlowerData, GrowthData>()
            .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)
            .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (growthQuery.IsEmpty)
            return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        state.Dependency = new FlowerGrowthJob
        {
            flowerLookup = SystemAPI.GetComponentLookup<FlowerData>(true),
            ecb = ecb
        }.ScheduleParallel(growthQuery, state.Dependency);
    }
}

[BurstCompile]
public partial struct FlowerGrowthJob : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<FlowerData> flowerLookup;
    public EntityCommandBuffer.ParallelWriter ecb;

    void Execute(
        [ChunkIndexInQuery] int sortKey,
        Entity entity,
        ref GrowthData growData,
        EnabledRefRW<GrowthData> grownEnabled)
    {
        if (!grownEnabled.ValueRO)
            return;

        if (growData.growth < growData.required)
        {
            growData.growth += growData.increment;
            return;
        }

        var flowerData = flowerLookup[entity];
        switch (flowerData.type)
        {
            case FlowerType.Dandelion:
                flowerData.pollen = 10f;
                flowerData.nectar = 10f;
                break;
            case FlowerType.Tulip:
                flowerData.pollen = 20f;
                flowerData.nectar = 5f;
                break;
            case FlowerType.Sunflower:
                flowerData.pollen = 50f;
                flowerData.nectar = 1f;
                break;
            default:
                flowerData.pollen = 10f;
                flowerData.nectar = 1f;
                break;
        }

        flowerData.owner = Entity.Null;
        ecb.SetComponent(sortKey, entity, flowerData);
        ecb.SetComponentEnabled<FlowerData>(sortKey, entity, true);
        growData.growth = 0;
        grownEnabled.ValueRW = false;
    }
}

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(GridSystem))]
[UpdateAfter(typeof(PollenCollectorSystem))]
public partial struct ScoutSystem : ISystem  
{
    private EntityQuery pureScoutQuery;
    private EntityQuery hybridScoutQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Scout>();
        state.RequireForUpdate<GridData>();
    
        pureScoutQuery = SystemAPI.QueryBuilder()
            .WithAll<Scout>()
            .WithAll<LocalTransform>()
            .WithDisabled<BeeMovementData>()
            .WithNone<PollenCollector>()
            .Build();

        hybridScoutQuery = SystemAPI.QueryBuilder()
            .WithAll<Scout, PollenCollector, NeedsFlowerAssignment>()
            .WithAll<LocalTransform>()
            .WithDisabled<BeeMovementData>()
            .Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gridData = SystemAPI.GetSingleton<GridData>();
        if (gridData.Grid.Count() == 0) return;
        
        int cellSize = SystemAPI.GetSingleton<GridData>().cellSize;
        uint randomSeed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;

        var job1 = new FindScoutTargetJob
        {
            cellSize = cellSize,
            randomSeed = randomSeed,
            seedOffset = 0,
            discoveredCells = gridData.DiscoveredCells 
        };
        var handle1 = job1.ScheduleParallel(pureScoutQuery, state.Dependency);


        var job2 = new FindScoutTargetJob
        {
            cellSize = cellSize,
            randomSeed = randomSeed,
            seedOffset = 10000,
            discoveredCells = gridData.DiscoveredCells
        };

        var handle2 = job2.ScheduleParallel(hybridScoutQuery, state.Dependency);
        state.Dependency = JobHandle.CombineDependencies(handle1, handle2);
    }
}


[BurstCompile]
public partial struct FindScoutTargetJob : IJobEntity
{
    public int cellSize;
    public uint randomSeed;
    public uint seedOffset;
    
    [ReadOnly] 
    public NativeParallelHashMap<int2, bool> discoveredCells;

    void Execute(
        Entity beeEntity,
        ref BeeMovementData beeMovementData, 
        in LocalTransform transform,
        EnabledRefRW<BeeMovementData> movementEnabled) 
    {
        var random = Random.CreateFromIndex(randomSeed + (uint)beeEntity.Index + seedOffset);
        float3 beePos = transform.Position;
        int2 beeCell = new int2((int)math.floor(beePos.x / cellSize), (int)math.floor(beePos.z / cellSize));
        
        for (int i = 0; i < 15; i++)
        {
            int2 randomOffset = new int2(random.NextInt(-15, 16), random.NextInt(-15, 16));
            int2 checkCell = beeCell + randomOffset;

            if (!discoveredCells.ContainsKey(checkCell))
            {
                float3 targetPos = new float3((checkCell.x * cellSize) + (cellSize * 0.5f), beePos.y, (checkCell.y * cellSize) + (cellSize * 0.5f));
                beeMovementData.moveLocation = targetPos;
    
                movementEnabled.ValueRW = true; 
                break;
            }
        }
    }
}
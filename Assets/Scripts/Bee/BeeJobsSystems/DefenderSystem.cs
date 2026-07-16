using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;


[UpdateAfter(typeof(PollenCollectorSystem))]

public partial struct DefenderSystem : ISystem
{
  
    private EntityQuery pureDefenderQuery;
    private EntityQuery CollectorDefenderQuery;
    private EntityQuery AttackerDefenderQuery;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridData>();
        pureDefenderQuery = SystemAPI.QueryBuilder()
        .WithAll<Defender, LocalTransform>()
        .WithDisabled<BeeMovementData>().
        WithNone<Attacker, PollenCollector>().
        Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gridData = SystemAPI.GetSingleton<GridData>();
        
        if (gridData.Grid.Count() == 0) return;
        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
        uint randomSeed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
        var PDjob = new PureDefenderJob
        {
            discoveredCells = gridData.DiscoveredCells,
            cellSize = cellSize,
            randomSeed = randomSeed,
            seedOffset = 0

        };
        var handle1 = PDjob.ScheduleParallel(pureDefenderQuery, state.Dependency);


    }
}
[BurstCompile]
public partial struct PureDefenderJob : IJobEntity
{
    [ReadOnly] 
    public NativeParallelHashMap<int2, bool> discoveredCells;
    public uint randomSeed;
    public uint seedOffset;
    public int cellSize;
    void Execute(
        Entity beeEntity,
        ref BeeMovementData beeMovementData,
        in LocalTransform transform,
        EnabledRefRW<BeeMovementData> movementEnabled
    )
    {
         var random = Random.CreateFromIndex(randomSeed + (uint)beeEntity.Index + seedOffset);
        float3 beePos = transform.Position;
        int2 beeCell = new int2((int)math.floor(beePos.x / cellSize), (int)math.floor(beePos.z / cellSize));
        
        for (int i = 0; i < 15; i++)
        {
            int2 randomOffset = new int2(random.NextInt(-15, 16), random.NextInt(-15, 16));
            int2 checkCell = beeCell + randomOffset;
            
            if (discoveredCells.ContainsKey(checkCell))
   
            {
                bool isBorder = 
                !discoveredCells.ContainsKey(checkCell + new int2(1, 0)) ||
                !discoveredCells.ContainsKey(checkCell + new int2(-1, 0)) ||
                !discoveredCells.ContainsKey(checkCell + new int2(0, 1)) ||
                !discoveredCells.ContainsKey(checkCell + new int2(0, -1));
                if(!isBorder) continue;
                float3 targetPos = new float3((checkCell.x * cellSize) + (cellSize * 0.5f), beePos.y, (checkCell.y * cellSize) + (cellSize * 0.5f));
                beeMovementData.moveLocation = targetPos;
    
                movementEnabled.ValueRW = true; 
                break;
            }
            /// add actual defending
            /// add behavior related to random ass bees
        }
    }
}
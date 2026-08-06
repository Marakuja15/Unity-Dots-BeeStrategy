using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(GridSystem))]
[UpdateBefore(typeof(ScoutSystem))]
public partial struct PollenCollectorSystem : ISystem 
{
    private EntityQuery pollenCollectorQuery;


    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    {
        state.RequireForUpdate<PollenCollector>();
        state.RequireForUpdate<GridData>();
        
        
  
        pollenCollectorQuery = SystemAPI.QueryBuilder()
            .WithAll<PollenCollector, NeedsFlowerAssignment>()
            .WithAllRW<BeeMovementData, BeeData>()
            .WithAll<LocalTransform>()
            .Build();
 
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)  
    {
        var gridData = SystemAPI.GetSingleton<GridData>();
        if (gridData.Grid.Count() == 0) return;

        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
        var takenFlowers = new NativeHashSet<Entity>(64, Allocator.TempJob);

        var flowerLookup = SystemAPI.GetComponentLookup<FlowerData>(false);
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        

        var job = new PollenCollectorJob
        {
            grid = gridData.Grid,
            discoveredCells = gridData.DiscoveredCells,
            cellSize = cellSize,
            takenFlowers = takenFlowers,
            flowerLookup = flowerLookup,
            transformLookup = transformLookup,
        };
        


        state.Dependency = job.Schedule(pollenCollectorQuery, state.Dependency);
        takenFlowers.Dispose(state.Dependency); 
        state.Dependency.Complete();
    }
}

[BurstCompile]
public partial struct PollenCollectorJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<int2, Entity> grid;
    [ReadOnly] public NativeParallelHashMap<int2, bool> discoveredCells;
    public int cellSize;
    
    public NativeHashSet<Entity> takenFlowers;
    public ComponentLookup<FlowerData> flowerLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;

    void Execute(
        Entity beeEntity,
        ref BeeMovementData beeMovementData,
        ref BeeData beeData,
        in LocalTransform transform,
        EnabledRefRW<NeedsFlowerAssignment> needsFlowerEnabled,
        EnabledRefRW<ReturnToHive> returnToHiveEnabled,
        DynamicBuffer<PollenStorage> pollenBuffer)
    {
  
        float total = 0;
        for (int i = 0; i < pollenBuffer.Length; i++)
            total += pollenBuffer[i].Amount;
        if (total >= beeData.maxPollen)
        {
            needsFlowerEnabled.ValueRW = false;
            returnToHiveEnabled.ValueRW = true;
            return; 
        }
        float3 beePos = transform.Position;
        int2 beeCell = new int2((int)math.floor(beePos.x / cellSize), (int)math.floor(beePos.z / cellSize));

        float closestDist = float.MaxValue;
        Entity closestFlower = Entity.Null;
        float3 closestPos = float3.zero;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                int2 checkCell = beeCell + new int2(x, z);
                if (!discoveredCells.ContainsKey(checkCell)) continue;
                
                if (grid.TryGetFirstValue(checkCell, out Entity flowerEntity, out var it))
                {
                    do
                    {
                        if (takenFlowers.Contains(flowerEntity)) continue;
                        float3 flowerPos = transformLookup[flowerEntity].Position;
                        float dist = math.distance(beePos, flowerPos);
                        
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestFlower = flowerEntity;
                            closestPos = flowerPos;
                        }
                    } while (grid.TryGetNextValue(out flowerEntity, ref it));
                }
            }
        }

        if (closestFlower != Entity.Null)
        {
            var flower = flowerLookup[closestFlower];
            flower.owner = beeEntity;
            flowerLookup[closestFlower] = flower;
            beeMovementData.moveLocation = closestPos;
            needsFlowerEnabled.ValueRW = false;
            takenFlowers.Add(closestFlower);
        }

   
    }
}
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(PollenCollectorSystem))]
public partial class GridSystem : SystemBase
{
   

   

    protected override void OnCreate()
    {
        RequireForUpdate<FlowerData>();
        RequireForUpdate<GridSystemData>();
        
        var gridData = new GridData
        {
            Grid = new NativeParallelMultiHashMap<int2, Entity>(1000, Allocator.Persistent),
            FlowerPositions = new NativeParallelMultiHashMap<int2, float3>(1000, Allocator.Persistent),
            DiscoveredCells = new NativeParallelHashMap<int2, bool>(10000, Allocator.Persistent),
            BeesInCell = new NativeParallelMultiHashMap<int2, Entity>(1000, Allocator.Persistent),
            TeamHives = new NativeParallelHashMap<byte, Entity>(1000, Allocator.Persistent)
        };
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, gridData);
    }

    protected override void OnUpdate()
    {
        ref var gridData = ref SystemAPI.GetSingletonRW<GridData>().ValueRW;

        gridData.Grid.Clear();
        gridData.FlowerPositions.Clear();
        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
   
        int flowerCount = 0;
        foreach (var _ in SystemAPI.Query<RefRO<FlowerData>>())
            flowerCount++;

        if (gridData.Grid.Capacity < flowerCount)
        {
            gridData.Grid.Capacity = flowerCount;
            gridData.FlowerPositions.Capacity = flowerCount;
            
        }
        
        Dependency = new BuildGridJob
        {
            cellSize = cellSize,
            gridWriter = gridData.Grid.AsParallelWriter(),
            posWriter = gridData.FlowerPositions.AsParallelWriter()
        
        }.ScheduleParallel(Dependency);

        Dependency.Complete();
    }
    protected override void OnDestroy()
    {
   
        if (SystemAPI.TryGetSingleton<GridData>(out var gridData))
        {
            if (gridData.Grid.IsCreated) gridData.Grid.Dispose();
            if (gridData.FlowerPositions.IsCreated) gridData.FlowerPositions.Dispose();
            if (gridData.DiscoveredCells.IsCreated) gridData.DiscoveredCells.Dispose();
            if (gridData.BeesInCell.IsCreated) gridData.BeesInCell.Dispose();
            if (gridData.TeamHives.IsCreated) gridData.TeamHives.Dispose();
        }
    }
    
}

[BurstCompile]
public partial struct BuildGridJob : IJobEntity
{
    public int cellSize;
    public NativeParallelMultiHashMap<int2, Entity>.ParallelWriter gridWriter;
    public NativeParallelMultiHashMap<int2, float3>.ParallelWriter posWriter;

    void Execute(in FlowerData flower, in LocalTransform transform, Entity entity)
    {   
        if (flower.owner != Entity.Null) return;
        
        int2 cell = new int2(
            (int)math.floor(transform.Position.x / cellSize),
            (int)math.floor(transform.Position.z / cellSize)
        );
        
        gridWriter.Add(cell, entity);
        posWriter.Add(cell, transform.Position);
    }
}

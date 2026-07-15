using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;


[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(BeeFlowerPickerSystem))]
public partial class GridSystem : SystemBase
{
    public NativeParallelMultiHashMap<int2, Entity> Grid;
    public NativeParallelMultiHashMap<int2, float3> FlowerPositions;
  
  
 


    protected override void OnCreate()
    {
        Grid = new NativeParallelMultiHashMap<int2, Entity>(1000, Allocator.Persistent);
        FlowerPositions = new NativeParallelMultiHashMap<int2, float3>(1000, Allocator.Persistent);
        RequireForUpdate<FlowerData>();
        RequireForUpdate<GridSystemData>();
    }

    protected override void OnUpdate()
    {
        Grid.Clear();
        FlowerPositions.Clear();

       
        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;
   
        int flowerCount = 0;
        foreach (var _ in SystemAPI.Query<RefRO<FlowerData>>())
            flowerCount++;

        if (Grid.Capacity < flowerCount)
        {
            Grid.Capacity = flowerCount;
            FlowerPositions.Capacity = flowerCount;
        }

    
        new BuildGridJob
        {
            cellSize = cellSize,
            gridWriter = Grid.AsParallelWriter(),
            posWriter = FlowerPositions.AsParallelWriter()
        }.ScheduleParallel();
    }

    protected override void OnDestroy()
    {
        if (Grid.IsCreated) Grid.Dispose();
        if (FlowerPositions.IsCreated) FlowerPositions.Dispose();
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

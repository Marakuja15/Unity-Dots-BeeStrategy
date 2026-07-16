using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public struct GridData : IComponentData
{
    public NativeParallelMultiHashMap<int2, Entity> Grid;
    public NativeParallelMultiHashMap<int2, float3> FlowerPositions;
    public NativeParallelHashMap<int2, bool> DiscoveredCells;

    public NativeParallelMultiHashMap<int2, Entity> BeesInCell;

    public NativeParallelMultiHashMap<int2, Entity> Cities; /// implement cities and the capital
}
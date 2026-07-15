
using Unity.Entities;

using Unity.Mathematics;
using Unity.Collections;
public struct GridCellData : IComponentData
{
    public bool discovered;
    public int2 cellIndex;

 

}

using Unity.Entities;
using Unity.Mathematics;

public struct BeeHiveData : IComponentData
{

    public int citiyDefenders;
    public float storedPollen;
    public int citizens;
    public float3 entrance;
}
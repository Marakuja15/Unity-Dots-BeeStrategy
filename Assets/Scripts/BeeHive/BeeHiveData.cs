
using Unity.Entities;
using Unity.Mathematics;

public struct BeeHiveData : IComponentData
{

    public int citiyDefenders;
    public int citizens;

    public int conversionWorkers;
    public float3 entrance;

    public float storedNectar;
    public float storedWax;
    public float storedHoney;
    public float waxRatio;
}
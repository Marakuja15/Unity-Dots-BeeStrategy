using Unity.Entities;

public struct HiveResources : IComponentData
{
    public float storedNectar;
    public float storedWax;
    public float storedHoney;
    public float waxRatio;
}

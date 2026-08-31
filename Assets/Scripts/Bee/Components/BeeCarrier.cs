using Unity.Entities;

public struct BeeCarrier : IComponentData
{
    public float maxPollen;
    public float collectedNectar;
    public float maxNectar;
    public float collectSpeed;


}
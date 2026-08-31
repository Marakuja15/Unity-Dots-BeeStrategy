using Unity.Entities;

public struct HiveProduction : IComponentData
{
    public int TargetBeesPer5Min; 
    public float SpawnTimer;    
    public bool IsCapital; 
}

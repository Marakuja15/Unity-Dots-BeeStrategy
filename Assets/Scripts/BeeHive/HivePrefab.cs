using Unity.Entities;
 struct HivePrefab : IComponentData
{
    public Entity Value;
}

public struct InitialHiveSpawnData : IComponentData
{
    public Entity Prefab;
    public Unity.Mathematics.float3 Position;
}

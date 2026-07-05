using Unity.Entities;
using Unity.Mathematics;
public struct FlowerSpawnerData : IComponentData
{
    public float3 spawnLeft;
    public float3 spawnRight;
    public Entity noPollenPrefab;
    public Entity prefab;
    public int numToSpawn;

}
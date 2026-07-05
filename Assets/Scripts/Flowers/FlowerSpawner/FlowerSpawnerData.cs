using Unity.Entities;
using Unity.Mathematics;
public struct FlowerSpawnerData : IComponentData
{
    public float3 spawnLeft;
    public float3 spawnRight;
    public Entity unGrownPrefab;
    public Entity prefab;
    public int numToSpawn;

}
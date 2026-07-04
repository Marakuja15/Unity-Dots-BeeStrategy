using Unity.Entities;
using Unity.Mathematics;


public struct SpawnerData : IComponentData
{
    public float3 spawnLocation;

    public Entity prefab;

    public int numOfEntities;
    public int removeNumOfEntities;

    public Random random;


    public float3 min;
    public float3 max;



 
}
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class FlowerSpawnerAuthoring : MonoBehaviour
{
    public GameObject noPollenPrefab;
    public GameObject prefab;

    public float3 spawnLeft;
    public float3 spawnRight;

    public int numToSpawn;
    class Baker : Baker<FlowerSpawnerAuthoring>
    {
        public override void Bake(FlowerSpawnerAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new FlowerSpawnerData
            {
                spawnLeft = authoring.spawnLeft,
                spawnRight = authoring.spawnRight,
                noPollenPrefab = GetEntity(authoring.noPollenPrefab, TransformUsageFlags.Dynamic),
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                numToSpawn = authoring.numToSpawn
            });
        }
    }
}
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class SpawnerAuthoring : MonoBehaviour
{
    public Vector3 spawnLocation;
    public GameObject prefab;

    public uint randomSeed = 1;
    public Vector3 min;
    public Vector3 max;
    // Baker ukryty wewnątrz klasy Authoring dla porządku
    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new SpawnerData
            {
               spawnLocation = authoring.spawnLocation,
               prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            
               random = new Unity.Mathematics.Random(authoring.randomSeed),
               min = authoring.min,
               max = authoring.max
            });
        }
    }
}
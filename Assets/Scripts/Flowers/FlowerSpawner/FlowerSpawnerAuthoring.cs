using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class FlowerSpawnerAuthoring : MonoBehaviour
{
    public GameObject unGrownPrefab;
    public GameObject prefab;

    public float3 spawnLeft;
    public float3 spawnRight;
    class Baker : Baker<FlowerSpawnerAuthoring>
    {
        public override void Bake(FlowerSpawnerAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new SpawnerData
            {
                
            });
        }
    }
}
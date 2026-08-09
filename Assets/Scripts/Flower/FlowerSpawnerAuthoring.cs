using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class FlowerSpawnerAuthoring : MonoBehaviour
{
    public GameObject noPollenPrefab;
    public GameObject prefab;
    public GameObject floorPrefab;
    public int numToSpawn;
    class Baker : Baker<FlowerSpawnerAuthoring>
    {
        public override void Bake(FlowerSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic); 

            float3 calculatedLeft = float3.zero;
            float3 calculatedRight = float3.zero;

            if (authoring.floorPrefab != null)
            {
                Vector3 floorPos = authoring.floorPrefab.transform.position;
                Vector3 floorScale = authoring.floorPrefab.transform.localScale;
                
                float halfX = floorScale.x / 2f;
                float halfZ = floorScale.z / 2f;

                calculatedLeft = new float3(floorPos.x - halfX, 0, floorPos.z - halfZ);
                calculatedRight = new float3(floorPos.x + halfX, 0, floorPos.z + halfZ);
            }

            AddComponent(entity, new FlowerSpawnerData
            {
                spawnLeft = calculatedLeft,
                spawnRight = calculatedRight,
                noPollenPrefab = GetEntity(authoring.noPollenPrefab, TransformUsageFlags.Dynamic),
                prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                numToSpawn = authoring.numToSpawn,
                floorPrefab = GetEntity(authoring.floorPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
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
                var floorRenderer = authoring.floorPrefab.GetComponent<Renderer>();
                if (floorRenderer != null)
                {
                    Bounds floorBounds = floorRenderer.bounds;
                    calculatedLeft = new float3(floorBounds.min.x, 0f, floorBounds.min.z);
                    calculatedRight = new float3(floorBounds.max.x, 0f, floorBounds.max.z);
                }
                else
                {
                    var meshFilter = authoring.floorPrefab.GetComponent<MeshFilter>();
                    Vector3 localSize = meshFilter != null && meshFilter.sharedMesh != null
                        ? meshFilter.sharedMesh.bounds.size
                        : Vector3.one;
                    Vector3 floorPos = authoring.floorPrefab.transform.position;
                    Vector3 worldScale = authoring.floorPrefab.transform.lossyScale;
                    float halfX = localSize.x * Mathf.Abs(worldScale.x) * 0.5f;
                    float halfZ = localSize.z * Mathf.Abs(worldScale.z) * 0.5f;

                    calculatedLeft = new float3(floorPos.x - halfX, 0f, floorPos.z - halfZ);
                    calculatedRight = new float3(floorPos.x + halfX, 0f, floorPos.z + halfZ);
                }
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



using Unity.Entities;

using UnityEngine;

public class HiveAuthoring : MonoBehaviour
{

    public GameObject beeHivePrefab;

    class Baker : Baker<HiveAuthoring>
    {
        public override void Bake(HiveAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var hivePrefab = GetEntity(authoring.beeHivePrefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new HivePrefab
            {
                Value = hivePrefab
            });
            AddComponent(entity, new InitialHiveSpawnData
            {
                Prefab = hivePrefab,
                Position = authoring.transform.position
            });
        }
    }
}

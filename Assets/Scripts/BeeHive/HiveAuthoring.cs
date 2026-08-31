

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
            AddComponent(entity, new HivePrefab
            {
                Value = GetEntity(authoring.beeHivePrefab, TransformUsageFlags.Dynamic)
    
            });
    
           
     
        }
    }
}
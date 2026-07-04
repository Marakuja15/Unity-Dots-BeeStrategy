
using System;
using Unity.Entities;
using UnityEngine;

public class FlowerAuthoring : MonoBehaviour
{
    public float pollen;
    public FlowerType type;
    public int growth;
    public int required;
    public int increment;
    class Baker : Baker<FlowerAuthoring>
    {
        public override void Bake(FlowerAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new FlowerData
            {
                pollen = authoring.pollen,
                type = authoring.type,
                


              
            });
            AddComponent(entity, new GrowthData
            {
                growth = authoring.growth,
                required = authoring.required,
                increment = authoring.increment
            });
      
           
            SetComponentEnabled<FlowerData>(entity, false);
        }
    }
}
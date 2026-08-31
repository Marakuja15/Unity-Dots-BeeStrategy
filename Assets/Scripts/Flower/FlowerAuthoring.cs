
using System;
using Unity.Entities;
using UnityEngine;

public class FlowerAuthoring : MonoBehaviour
{

    public FlowerType type;
  

    class Baker : Baker<FlowerAuthoring>
    {
        public override void Bake(FlowerAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new FlowerData
            {
                pollen =  FlowerTypeInfo.GetPollen(authoring.type),
                type = authoring.type, 
            });
            AddComponent(entity, new GrowthData
            {
                growth = 0,
                required = FlowerTypeInfo.GetGrowthRequired(authoring.type),
                increment = FlowerTypeInfo.GetGrowthIncrement(authoring.type)
            });
      
            SetComponentEnabled<GrowthData>(entity, false);
            SetComponentEnabled<FlowerData>(entity, false);
        }
    }
}
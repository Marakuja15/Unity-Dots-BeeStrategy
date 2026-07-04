
using System;
using Unity.Entities;
using UnityEngine;

public class FlowerAuthoring : MonoBehaviour
{
    public float pollen;
    public FlowerType type;
   
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
      
         
            SetComponentEnabled<FlowerData>(entity, true);
        }
    }
}
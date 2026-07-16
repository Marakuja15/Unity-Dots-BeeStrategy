
using System;
using Unity.Entities;
using UnityEngine;

public class BeeHiveAuthoring : MonoBehaviour
{

    public FlowerType type;
  

    class Baker : Baker<BeeHiveAuthoring>
    {
        public override void Bake(BeeHiveAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
     
        }
    }
}
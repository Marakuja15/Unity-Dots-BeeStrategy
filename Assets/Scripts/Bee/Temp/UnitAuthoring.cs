using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class UnitAuthoring : MonoBehaviour
{
    public float speed;
    public float stoppingDistance;
    // Baker ukryty wewnątrz klasy Authoring dla porządku
    class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new UnitData
            {
                speed = authoring.speed,
                stoppingDistance = authoring.stoppingDistance

              
            });
      
            AddComponent(entity, new UnitMovementData());
            SetComponentEnabled<UnitMovementData>(entity, false);
        }
    }
}
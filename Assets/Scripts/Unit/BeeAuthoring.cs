using System.Numerics;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class BeeAuthoring : MonoBehaviour
{
    public float health;
    public float speed;

    public float wobbleFrequency;

    public float wobbleAmplitude;
    public float randomOffSet;
    class Baker : Baker<BeeAuthoring>
    {
        public override void Bake(BeeAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            AddComponent(entity, new BeeData
            {
                health = authoring.health,
               

              
            });
      
            AddComponent(entity, new BeeMovementData
            {
                speed = authoring.speed,
                wobbleFrequency = authoring.wobbleFrequency,
                wobbleAmplitude = authoring.wobbleAmplitude,
                randomOffSet = authoring.randomOffSet


            });
            SetComponentEnabled<BeeMovementData>(entity, false);
        }
    }
}
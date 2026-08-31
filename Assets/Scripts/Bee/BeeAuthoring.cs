using Unity.Entities;
using UnityEngine;

public class BeeAuthoring : MonoBehaviour
{
    public float health;
    public float speed;
    public float wobbleFrequency;
    public float wobbleAmplitude;
    public float randomOffSet;
    public float stopRadius;
    public float maxPollen;
    public float maxNectar;
    public float collectSpeed;

    class Baker : Baker<BeeAuthoring>
    {
        public override void Bake(BeeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BeeHealth
            {
                health = authoring.health
            });
            AddComponent(entity, new BeeCarrier
            {
                maxPollen = authoring.maxPollen,
                maxNectar = authoring.maxNectar,
                collectSpeed = authoring.collectSpeed
            });
            AddComponent(entity, new HiveAssignment());
            AddComponent(entity, new BeeMovementData
            {
                speed = authoring.speed,
                wobbleFrequency = authoring.wobbleFrequency,
                wobbleAmplitude = authoring.wobbleAmplitude,
                randomOffSet = authoring.randomOffSet,
                stopRadius = authoring.stopRadius
            });
            SetComponentEnabled<BeeMovementData>(entity, false);

            var buffer = AddBuffer<PollenStorage>(entity);
            foreach (FlowerType type in FlowerTypeInfo.GetAllTypes())
            {
                buffer.Add(new PollenStorage { Type = type, Amount = 0 });
            }
        }
    }
}
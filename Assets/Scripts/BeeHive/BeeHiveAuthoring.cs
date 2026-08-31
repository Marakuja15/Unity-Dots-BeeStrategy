using Unity.Entities;
using UnityEngine;

public class BeeHiveAuthoring : MonoBehaviour
{
    public int cityDefenders;
    public int citizens;
    public Vector3 entrance;

    class Baker : Baker<BeeHiveAuthoring>
    {
        public override void Bake(BeeHiveAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HiveResources());
            AddComponent(entity, new HivePopulation
            {
                defenders = authoring.cityDefenders,
                citizens = authoring.citizens
            });
            AddComponent(entity, new HiveEntrance
            {
                position = authoring.entrance
            });
            var buffer = AddBuffer<PollenStorage>(entity);
            foreach (FlowerType type in FlowerTypeInfo.GetAllTypes())
            {
                buffer.Add(new PollenStorage { Type = type, Amount = 0 });
            }
        }
    }
}
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
            AddComponent(entity, new HiveProduction
            {
                TargetBeesPer5Min = 10, // Domyślnie 10 pszczół na 5 minut
                SpawnTimer = 0f,
                IsCapital = false // Zmienimy to na true dla pierwszego ula w przyszłości
            });
            AddComponent(entity, new HiveInfrastructure { hasSchool = false });
            
            // Override the default HivePopulation to give us some UI test data
            SetComponent(entity, new HivePopulation
            {
                citizens = authoring.citizens,
                defenders = authoring.cityDefenders,
                conversionWorkers = 0,
                uneducatedBees = 5,
                educatedBees = 10
            });
            var buffer = AddBuffer<PollenStorage>(entity);
            foreach (FlowerType type in FlowerTypeInfo.GetAllTypes())
            {
                buffer.Add(new PollenStorage { Type = type, Amount = 0 });
            }
        }
    }
}
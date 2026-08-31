
using Unity.Entities;
using UnityEngine;


public class TeamsAuthoring : MonoBehaviour
{
    class Baker : Baker<TeamsAuthoring>
    {
        public override void Bake(TeamsAuthoring authoring)
        {
       
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            
            var buffer = AddBuffer<PollenStorage>(entity);
            foreach (FlowerType type in FlowerTypeInfo.GetAllTypes())
            {
                buffer.Add(new PollenStorage { Type = type, Amount = 0 });
            }
           
        }
    }
}
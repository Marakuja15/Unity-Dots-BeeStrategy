
using Unity.Entities;
using UnityEngine;

public class GridAuthoring : MonoBehaviour
{
    public int cellSize;

    class Baker : Baker<GridAuthoring>
    {
        public override void Bake(GridAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic); 
            AddComponent(entity, new GridSystemData
            {
                cellSize = authoring.cellSize
            });
        }
    }
}
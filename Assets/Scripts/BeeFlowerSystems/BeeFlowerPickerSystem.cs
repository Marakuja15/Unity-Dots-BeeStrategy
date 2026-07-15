using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(GridSystem))]
public partial class BeeFlowerPickerSystem : SystemBase
{
    private GridSystem m_GridSystem;
    protected override void OnCreate()
    {
        RequireForUpdate<PollenCollector>();
        RequireForUpdate<GridSystemData>();
        m_GridSystem = World.GetExistingSystemManaged<GridSystem>();
    }

    protected override void OnUpdate()
    {
  
      

        var grid = m_GridSystem.Grid;
        int cellSize = SystemAPI.GetSingleton<GridSystemData>().cellSize;

        if (grid.Count() == 0) return;

        var takenFlowers = new NativeHashSet<Entity>(64, Allocator.Temp);

        foreach (var (beeMovementData, transform, beeEntity)
        in SystemAPI.Query<
            RefRW<BeeMovementData>,
            RefRO<LocalTransform>>()
            .WithAll<PollenCollector, NeedsFlowerAssignment>()
            .WithDisabled<BeeMovementData>()
            .WithEntityAccess())
        {
            float3 beePos = transform.ValueRO.Position;
            int2 beeCell = new int2(
                (int)math.floor(beePos.x / cellSize),
                (int)math.floor(beePos.z / cellSize)
            );

            float closestDist = float.MaxValue;
            Entity closestFlower = Entity.Null;
            float3 closestPos = float3.zero;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    int2 checkCell = beeCell + new int2(x, z);
                    if (grid.TryGetFirstValue(checkCell, out Entity flowerEntity, out var it))
                    {
                        do
                        {
                            if (takenFlowers.Contains(flowerEntity)) continue;

                            float3 flowerPos = SystemAPI.GetComponent<LocalTransform>(flowerEntity).Position;
                            float dist = math.distance(beePos, flowerPos);
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closestFlower = flowerEntity;
                                closestPos = flowerPos;
                            }
                        } while (grid.TryGetNextValue(out flowerEntity, ref it));
                    }
                }
            }

            if (closestFlower == Entity.Null) continue;

            var flower = SystemAPI.GetComponentRW<FlowerData>(closestFlower);
            flower.ValueRW.owner = beeEntity;
            beeMovementData.ValueRW.moveLocation = closestPos;
            SystemAPI.SetComponentEnabled<BeeMovementData>(beeEntity, true);
            SystemAPI.SetComponentEnabled<NeedsFlowerAssignment>(beeEntity, false);
            takenFlowers.Add(closestFlower);
        }

        takenFlowers.Dispose();
    }
}
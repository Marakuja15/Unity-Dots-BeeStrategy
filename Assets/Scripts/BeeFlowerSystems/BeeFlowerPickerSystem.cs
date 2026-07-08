using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine.Analytics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class BeeFlowerPickerSystem : SystemBase
{



    protected override void OnCreate()
    {
        RequireForUpdate<PollenCollector>();
       
    }
    protected override void OnUpdate()
    {
        var flowerPositions = new NativeList<float3>(Allocator.Temp);
        var flowerEntities = new NativeList<Entity>(Allocator.Temp);
        // Make a grid system and restrict the loop to search through one grid
        foreach (var (flowerData, transform, enabledState, entity)
        in SystemAPI.Query<RefRO<FlowerData>,
        RefRO<LocalTransform>,
        EnabledRefRO<FlowerData>>().WithEntityAccess())
        {
            if (flowerData.ValueRO.owner != Entity.Null) continue;
            flowerPositions.Add(transform.ValueRO.Position);
            flowerEntities.Add(entity);

        }
        if (flowerPositions.Length == 0 || flowerEntities.Length == 0)
        {
            flowerPositions.Dispose();
            flowerEntities.Dispose();
            return;
        }

        foreach (var (beeMovementData, transform, beeEntity)
        in SystemAPI.Query<
        RefRW<BeeMovementData>,
        RefRO<LocalTransform>>()
        .WithAll<PollenCollector, NeedsFlowerAssignment>() 
        .WithDisabled<BeeMovementData>()                 
        .WithEntityAccess())
        {
            float closestDist = float.MaxValue;
            Entity closestFlower = Entity.Null;
            int closestIndex = -1;
            for (int i = 0; i < flowerPositions.Length; i++)
            {
                float dist = math.distance(transform.ValueRO.Position, flowerPositions[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestFlower = flowerEntities[i];
                    closestIndex = i;
                }

            }
            if (closestIndex == -1) break;

            var flower = SystemAPI.GetComponentRW<FlowerData>(closestFlower);
            flower.ValueRW.owner = beeEntity;   
            beeMovementData.ValueRW.moveLocation = flowerPositions[closestIndex];
            SystemAPI.SetComponentEnabled<BeeMovementData>(beeEntity, true);
            SystemAPI.SetComponentEnabled<NeedsFlowerAssignment>(beeEntity, false);
            flowerPositions.RemoveAtSwapBack(closestIndex);
            flowerEntities.RemoveAtSwapBack(closestIndex);

        }
        flowerPositions.Dispose();
        flowerEntities.Dispose();


    }

}

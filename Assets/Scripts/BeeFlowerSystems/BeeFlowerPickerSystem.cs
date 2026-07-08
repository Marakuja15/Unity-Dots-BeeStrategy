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

    private InputSystem _inputControls;

    private Camera _mainCamera;

    private Queue<Vector2> _clickQueue = new Queue<Vector2>();

    protected override void OnCreate()
    {
        RequireForUpdate<BeeMovementData>();
        _inputControls = new InputSystem();
        _inputControls.Enable();
        _inputControls.Gameplay.Move.performed += OnMovePerformed;
 
     
    }

    protected override void OnStartRunning()
    {
        _mainCamera = Camera.main;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 pointerPos = _inputControls.Gameplay.PointerPosition.ReadValue<Vector2>();
        _clickQueue.Enqueue(pointerPos);

    }

    protected override void OnUpdate()
    {
        
        while(_clickQueue.Count > 0)
        {
         

           
         
            _clickQueue.Dequeue(); 
 

            var flowerPositions = new NativeList<float3>(Allocator.Temp);
            var flowerEntities = new NativeList<Entity>(Allocator.Temp);
            foreach (var(flowerData, transform, enabledState, entity)
            in SystemAPI.Query<RefRO<FlowerData>,
            RefRO<LocalTransform>,
            EnabledRefRO<FlowerData>>().WithEntityAccess())
            {
                if(flowerData.ValueRO.owner != Entity.Null) continue;
                flowerPositions.Add(transform.ValueRO.Position);
                flowerEntities.Add(entity);

            }
            if(flowerPositions.Length == 0 || flowerEntities.Length == 0)
            {
                flowerPositions.Dispose();
                flowerEntities.Dispose();
                continue;
            }

            foreach(var (beeData, beeMovementData,  transform,  EnabledState, beeEntity)
            in SystemAPI.Query<RefRW<BeeData>,
            RefRW<BeeMovementData>,
            RefRO<LocalTransform>,
            EnabledRefRW<BeeMovementData>>().WithEntityAccess().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                EnabledState.ValueRW = true;
                float closestDist = float.MaxValue;
                Entity closestFlower = Entity.Null;
                int closestIndex = -1;
                for(int i = 0; i < flowerPositions.Length; i++)
                {
                    float dist = math.distance(transform.ValueRO.Position, flowerPositions[i]);
                    if( dist < closestDist )
                    {
                        closestDist = dist;
                        closestFlower = flowerEntities[i];
                        closestIndex = i;
                    }
               
                }
                if(closestIndex == - 1) break;
                
                var flower = SystemAPI.GetComponentRW<FlowerData>(closestFlower);
                flower.ValueRW.owner = beeEntity; 
                beeMovementData.ValueRW.moveLocation = flowerPositions[closestIndex];
                flowerPositions.RemoveAtSwapBack(closestIndex);
                flowerEntities.RemoveAtSwapBack(closestIndex);
                
            }
            flowerPositions.Dispose();
            flowerEntities.Dispose();
        }
        
    }
    protected override void OnDestroy()
    {
        _inputControls.Gameplay.Move.performed -= OnMovePerformed;
        _inputControls.Disable();
    }
}

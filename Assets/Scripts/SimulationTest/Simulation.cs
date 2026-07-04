using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class Simulation : SystemBase
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
         

            Vector2 pointerPos = _clickQueue.Dequeue();
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));
            float spreadRadius = 6.0f;
          
            Ray ray = _mainCamera.ScreenPointToRay(pointerPos);
            Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
            if(floorPlane.Raycast(ray, out float distance))
            {
                float3 target = ray.GetPoint(distance);
                foreach(var (UnitMovement,  EnabledState)
                 in SystemAPI.Query<RefRW<BeeMovementData>,
                 EnabledRefRW<BeeMovementData>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
                {
                    EnabledState.ValueRW = true;
                    float3 randomSphere = rand.NextFloat3Direction() * rand.NextFloat(0, spreadRadius);
                 
                    UnitMovement.ValueRW.moveLocation = target + randomSphere;
                }
            }
        }
    }
    protected override void OnDestroy()
    {
        _inputControls.Gameplay.Move.performed -= OnMovePerformed;
        _inputControls.Disable();
    }
}

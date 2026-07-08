// using Unity.Entities;
// using Unity.Burst;
// using Unity.Mathematics;
// using Unity.Collections;

// [BurstCompile]
// [UpdateInGroup(typeof(SimulationSystemGroup))]

// public partial struct RemoveSystem : ISystem
// {
//     private EntityQuery _unitQuery;
//     [BurstCompile]
    
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<SpawnerData>(); 
//         state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
//         _unitQuery = SystemAPI.QueryBuilder().WithAll<UnitData>().WithNone<Prefab>().Build();
//     }

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         if(!SystemAPI.TryGetSingletonEntity<SpawnerData>(out Entity spawnerEntity)) return;
//         RefRW<SpawnerData> spawner = SystemAPI.GetComponentRW<SpawnerData>(spawnerEntity);
//         if(spawner.ValueRO.removeNumOfEntities < 1)  return;
//         var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
//         var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
//         using var entities = _unitQuery.ToEntityArray(Allocator.Temp);

//         int deleted = 0;
//         for (int i = 0; i < entities.Length; i++)
//         {
//             if (entities[i] == spawner.ValueRO.prefab) 
//                 continue;

//             ecb.DestroyEntity(entities[i]);
//             deleted++;

//             if (deleted >= spawner.ValueRO.removeNumOfEntities) 
//                 break; 
//         }

//         spawner.ValueRW.removeNumOfEntities -= deleted;  
        
//     }
// }
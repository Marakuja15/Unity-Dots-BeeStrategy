using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public partial struct TeamSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity();
        DynamicBuffer<TeamDataElement> teamsBuffer = state.EntityManager.AddBuffer<TeamDataElement>(entity);
        teamsBuffer.Add(new TeamDataElement { TeamID = 0, BeeCount = 0, StoredPollen = 0 });
        teamsBuffer.Add(new TeamDataElement { TeamID = 1, BeeCount = 0, StoredPollen = 0 });
    }
    public void OnUpdate(ref SystemState state)
    {

        // TODO: old system with one pollen type. Change to multiple pollen types and maybe make diff system for it.
        // if (!SystemAPI.TryGetSingletonBuffer<TeamDataElement>(out var teamsBuffer)) return;
        // for (int i = 0; i < teamsBuffer.Length; i++)
        // {
        //     var team = teamsBuffer[i];
        //     team.StoredPollen = 0;
        //     teamsBuffer[i] = team;
        // }
        // foreach (var (beeHiveData,teamData) in 
        // SystemAPI.Query<RefRW<BeeHiveData>,
        // RefRO<TeamData>>())
        // {
        //     byte id = teamData.ValueRO.TeamID;
        //     var teamStats = teamsBuffer[id];
        //     teamStats.StoredPollen += (int)beeHiveData.ValueRO.storedPollen;
        //     teamsBuffer[id] = teamStats;
        // }
       
    }
}   
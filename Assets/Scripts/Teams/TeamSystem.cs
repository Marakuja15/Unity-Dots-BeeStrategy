using Unity.Entities;
using Unity.Burst;

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
}
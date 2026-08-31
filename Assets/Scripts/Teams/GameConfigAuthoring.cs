using Unity.Entities;
using UnityEngine;

public class GameConfigAuthoring : MonoBehaviour
{
    public byte playerTeamID = 0;
    public int numberOfTeams = 2;

    class Baker : Baker<GameConfigAuthoring>
    {
        public override void Bake(GameConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);


            AddComponent(entity, new PlayerData
            {
                TeamID = authoring.playerTeamID
            });

    
            var buffer = AddBuffer<TeamDataElement>(entity);
            for (int i = 0; i < authoring.numberOfTeams; i++)
            {
                buffer.Add(new TeamDataElement
                {
                    TeamID = (byte)i,
                    BeeCount = 0,
                    constructionWorkers = 0,
                    storedWax = 0,
                    storedHoney = 0
                });
            }
        }
    }
}
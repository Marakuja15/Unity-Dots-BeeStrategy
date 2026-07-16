
using Unity.Entities;
public enum Team
{
    player,
    team1,
    team2
}



public struct BeeData : IComponentData
{
  
    public float health;
    public float collectedPollen;
    public float maxPollen;




}
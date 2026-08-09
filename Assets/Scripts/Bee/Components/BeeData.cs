

using Unity.Entities;



public struct BeeData : IComponentData
{
  
    public float health;

    public float maxPollen;

    public Entity currentHive;
    public float collectedNectar;
    public float maxNectar;
    public float collectSpeed;




}
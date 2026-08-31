using Unity.Entities;

public struct TeamDataElement : IBufferElementData
{
    public byte TeamID;      
    public int BeeCount;      

    public Entity QueenEntity;  

    public int constructionWorkers;

    public float storedWax;
    public float storedHoney;

}
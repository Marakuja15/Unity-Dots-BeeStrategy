using Unity.Entities;

public struct TeamDataElement : IBufferElementData
{
    public byte TeamID;      
    public int BeeCount;      
    public int StoredPollen; 
    public Entity QueenEntity;  

}
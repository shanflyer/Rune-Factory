public interface IDataArray<T> where T : IGameData
{
    public T[] DataList { get; }
 
}
public interface ISingleDataArray<T> where T : IGameData{ }
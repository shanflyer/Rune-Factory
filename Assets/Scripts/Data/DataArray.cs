public interface IDataArray<T> where T : IGameData
{
    public T[] DataList { get; }
}
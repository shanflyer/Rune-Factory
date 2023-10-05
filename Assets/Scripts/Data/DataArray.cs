using NUnit.Framework;
using System.Collections.Generic;

public interface IDataArray<T> where T : IGameData
{
    public List<T> DataList { get; }
}
using NUnit.Framework;
using System.Collections.Generic;

public interface IDataArray<T>
{
    public List<T> DataList { get; }
}
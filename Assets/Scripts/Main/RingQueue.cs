using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///     一个支持动态扩容的环形队列
///     - 入队 O(1)
///     - 出队 O(1)
///     - 索引访问 O(1)
/// </summary>
public class RingQueue<T> : IEnumerable<T>
{
    private T[] buffer;
    private int head; // 队首
    private int tail; // 队尾（下一个写入位置）

    public int Count { get; private set; }

    public int Capacity => buffer.Length;
    public bool IsEmpty => Count == 0;

    public RingQueue(int capacity = 16)
    {
        if (capacity < 1) capacity = 1;
        buffer = new T[capacity];
        head = 0;
        tail = 0;
        Count = 0;
    }

    /// <summary> 批量丢弃前 n 个元素（0..n-1） </summary>
    public void Discard(int n)
    {
        if (n <= 0) return;
        if (n > Count) n = Count;

        // 清理引用（避免托管类型内存泄露）
        for (var i = 0; i < n; i++) buffer[(head + i) % buffer.Length] = default;

        head = (head + n) % buffer.Length;
        Count -= n;
    }

    /// <summary> 入队（队尾添加） </summary>
    public void Enqueue(T item)
    {
        if (Count == buffer.Length)
            Expand();

        buffer[tail] = item;
        tail = (tail + 1) % buffer.Length;
        Count++;
    }

    /// <summary> 出队（队首移除） </summary>
    public T Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty");

        var item = buffer[head];
        buffer[head] = default; // 可选：清理引用，避免内存泄漏
        head = (head + 1) % buffer.Length;
        Count--;
        return item;
    }

    /// <summary> 查看队首但不移除 </summary>
    public T Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty");

        return buffer[head];
    }

    /// <summary> 按逻辑索引访问 </summary>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            return buffer[(head + index) % buffer.Length];
        }
        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            buffer[(head + index) % buffer.Length] = value;
        }
    }

    /// <summary> 扩容为原来的两倍 </summary>
    private void Expand()
    {
        var newCapacity = buffer.Length * 2;
        var newBuffer = new T[newCapacity];

        for (var i = 0; i < Count; i++) newBuffer[i] = buffer[(head + i) % buffer.Length];

        buffer = newBuffer;
        head = 0;
        tail = Count;
    }

    public void Clear()
    {
        Array.Clear(buffer, 0, buffer.Length);
        head = 0;
        tail = 0;
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return buffer[(head + i) % buffer.Length];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

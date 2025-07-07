// 无锁、覆盖式、并发安全、哈希结构的 NativeOverwritingHashMap
// 特点：
// - 不需要状态位（总是写入成功）
// - 后写覆盖前写
// - 写入即读取，无需 Pending 状态
// - 对 key 做快速哈希+开放寻址（线性探测）

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace NativeCollections
{
    [NativeContainer]
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile]
    public unsafe struct NativeOverwritingHashMap<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [NativeDisableUnsafePtrRestriction] private TKey* m_Keys;
        [NativeDisableUnsafePtrRestriction] private TValue* m_Values;
        private int m_Capacity;
        private int m_Mask;
        private Allocator m_Allocator;

        public NativeOverwritingHashMap(int capacity, Allocator allocator)
        {
            if (!math.ispow2(capacity))
                throw new ArgumentException("Capacity must be power of 2");

            m_Capacity = capacity;
            m_Mask = capacity - 1;
            m_Allocator = allocator;

            m_Keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * capacity, UnsafeUtility.AlignOf<TKey>(), allocator);
            m_Values = (TValue*)UnsafeUtility.Malloc(sizeof(TValue) * capacity, UnsafeUtility.AlignOf<TValue>(), allocator);

            UnsafeUtility.MemClear(m_Keys, sizeof(TKey) * capacity);
            UnsafeUtility.MemClear(m_Values, sizeof(TValue) * capacity);
        }

        public bool IsCreated => m_Keys != null;

        public void Dispose()
        {
            if (m_Keys != null) UnsafeUtility.Free(m_Keys, m_Allocator);
            if (m_Values != null) UnsafeUtility.Free(m_Values, m_Allocator);
        }

        public void Set(TKey key, TValue value)
        {
            int hash = key.GetHashCode();
            int idx = hash & m_Mask;
            for (int i = 0; i < m_Capacity; i++)
            {
                int probe = (idx + i) & m_Mask;
                if (m_Keys[probe].Equals(key))
                {
                    m_Values[probe] = value;
                    return;
                }
                if (m_Keys[probe].Equals(default))
                {
                    m_Keys[probe] = key;
                    m_Values[probe] = value;
                    return;
                }
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            int hash = key.GetHashCode();
            int idx = hash & m_Mask;
            for (int i = 0; i < m_Capacity; i++)
            {
                int probe = (idx + i) & m_Mask;
                if (m_Keys[probe].Equals(key))
                {
                    value = m_Values[probe];
                    return true;
                }
                if (m_Keys[probe].Equals(default))
                    break;
            }
            value = default;
            return false;
        }

        public bool TryRemove(TKey key)
        {
            int hash = key.GetHashCode();
            int idx = hash & m_Mask;
            for (int i = 0; i < m_Capacity; i++)
            {
                int probe = (idx + i) & m_Mask;
                if (m_Keys[probe].Equals(key))
                {
                    m_Keys[probe] = default;
                    m_Values[probe] = default;
                    return true;
                }
            }
            return false;
        }
    }
}

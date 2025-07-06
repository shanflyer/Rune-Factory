// 支持并行写入 + 并行读取 + TryRemove，模仿 Unity UnsafeParallelHashMap，实现桶式哈希 + 链式冲突处理结构

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace NativeCollections
{
    public enum TryGetResult
    {
        NotFound,        // 桶为空或无匹配项
        Found,           // 找到并且写入已完成
        PendingWrite     // 有 key 但写入尚未完成
    }
    [NativeContainer]
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public unsafe struct NativeConcurrentMap<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        [NativeDisableUnsafePtrRestriction] private int* m_Buckets;
        [NativeDisableUnsafePtrRestriction] private int* m_NextPtrs;
        [NativeDisableUnsafePtrRestriction] private TKey* m_Keys;
        [NativeDisableUnsafePtrRestriction] private TValue* m_Values;
        [NativeDisableUnsafePtrRestriction] private int* m_States;
        [NativeDisableUnsafePtrRestriction] private int* m_Count;

        private int m_Capacity;
        private int m_BucketCapacityMask;
        private Allocator m_Allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        internal static readonly SharedStatic<int> s_staticSafetyId = SharedStatic<int>.GetOrCreate<NativeConcurrentMap<TKey, TValue>>();
#endif

        public NativeConcurrentMap(int capacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = CollectionHelper.CreateSafetyHandle(allocator);
            CollectionHelper.SetStaticSafetyId<NativeConcurrentMap<TKey, TValue>>(ref m_Safety, ref s_staticSafetyId.Data);
#endif
            if (!math.ispow2(capacity))
                throw new ArgumentException("Capacity must be a power of 2");

            m_Allocator = allocator;
            m_Capacity = capacity;
            m_BucketCapacityMask = capacity - 1;

            m_Buckets = (int*)UnsafeUtility.Malloc(sizeof(int) * capacity, 4, allocator);
            m_NextPtrs = (int*)UnsafeUtility.Malloc(sizeof(int) * capacity, 4, allocator);
            m_Keys = (TKey*)UnsafeUtility.Malloc(sizeof(TKey) * capacity, UnsafeUtility.AlignOf<TKey>(), allocator);
            m_Values = (TValue*)UnsafeUtility.Malloc(sizeof(TValue) * capacity, UnsafeUtility.AlignOf<TValue>(), allocator);
            m_States = (int*)UnsafeUtility.Malloc(sizeof(int) * capacity, 4, allocator);
            m_Count = (int*)UnsafeUtility.Malloc(sizeof(int), 4, allocator);

            for (int i = 0; i < capacity; ++i)
                m_Buckets[i] = -1;

            UnsafeUtility.MemClear(m_NextPtrs, sizeof(int) * capacity);
            UnsafeUtility.MemClear(m_States, sizeof(int) * capacity);
            *m_Count = 0;
        }

        public bool IsCreated => m_Buckets != null;
        public int Count => *m_Count;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
            AtomicSafetyHandle.Release(m_Safety);
#endif
            if (m_Buckets != null) UnsafeUtility.Free(m_Buckets, m_Allocator);
            if (m_NextPtrs != null) UnsafeUtility.Free(m_NextPtrs, m_Allocator);
            if (m_Keys != null) UnsafeUtility.Free(m_Keys, m_Allocator);
            if (m_Values != null) UnsafeUtility.Free(m_Values, m_Allocator);
            if (m_States != null) UnsafeUtility.Free(m_States, m_Allocator);
            if (m_Count != null) UnsafeUtility.Free(m_Count, m_Allocator);
        }

        public bool TrySet(TKey key, TValue value)
        {
            int hash = key.GetHashCode();
            int bucket = hash & m_BucketCapacityMask;

            for (int i = Volatile.Read(ref m_Buckets[bucket]), steps = 0; i != -1 && steps++ < 128; i = m_NextPtrs[i])
            {
                int state = Volatile.Read(ref m_States[i]);
                if (state == 2 && m_Keys[i].Equals(key))
                {
                    m_Values[i] = value;
                    return true;
                }
            }

            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            int hash = key.GetHashCode();
            int bucket = hash & m_BucketCapacityMask;

            // 先检查：已有 key 且状态为已完成，直接失败
            for (int i = Volatile.Read(ref m_Buckets[bucket]), steps = 0; i != -1 && steps++ < 128; i = m_NextPtrs[i])
            {
                int state = Volatile.Read(ref m_States[i]);
                if (state == 2 && m_Keys[i].Equals(key))
                    return false;
            }

            // 分配插入槽位
            int idx = Interlocked.Increment(ref *m_Count) - 1;
            if (idx >= m_Capacity) return false;

            // 写入 key 
            m_Keys[idx] = key;

            // 标记为“写入中”
            Volatile.Write(ref m_States[idx], 1); // ✅ 写入前显式标记状态 1

            // 写入value
            m_Values[idx] = value;

            // 插入到链表
            int oldHead;
            do
            {
                oldHead = Volatile.Read(ref m_Buckets[bucket]);
                if (oldHead == idx) return false;
                m_NextPtrs[idx] = oldHead;
            } while (Interlocked.CompareExchange(ref m_Buckets[bucket], idx, oldHead) != oldHead);

            // ✅ 完成写入，设置为“完成状态”
            Thread.MemoryBarrier();
            Volatile.Write(ref m_States[idx], 2);

            return true;
        }

        public TryGetResult TryGetValue(TKey key, out TValue value)
        {
            int hash = key.GetHashCode();
            int bucket = hash & m_BucketCapacityMask;

            int steps = 0;
            for (int i = Volatile.Read(ref m_Buckets[bucket]); i != -1 && steps++ < 128; i = m_NextPtrs[i])
            {
                int state = Volatile.Read(ref m_States[i]);

                if (state == 2 && m_Keys[i].Equals(key))
                {
                    value = m_Values[i];
                    return TryGetResult.Found;
                }

                if (state == 1 && m_Keys[i].Equals(key))
                {
                    value = default;
                    return TryGetResult.PendingWrite;
                }
            }
            for (int i = 0; i < m_Capacity; ++i)
            {
                int state = Volatile.Read(ref m_States[i]);
                if ((state == 1 || state == 2) && m_Keys[i].Equals(key))
                {
                    value = default;
                    return (state == 2) ? TryGetResult.Found : TryGetResult.PendingWrite;
                }
            }


            value = default;
            return TryGetResult.NotFound; 
        }


        public bool TryRemove(TKey key)
        {
            int hash = key.GetHashCode();
            int bucket = hash & m_BucketCapacityMask;

            int prev = -1;
            for (int i = Volatile.Read(ref m_Buckets[bucket]), steps = 0; i != -1 && steps++ < 128; prev = i, i = m_NextPtrs[i])
            {
                if (Volatile.Read(ref m_States[i]) == 2 && m_Keys[i].Equals(key))
                {
                    if (prev < 0)
                        m_Buckets[bucket] = m_NextPtrs[i];
                    else
                        m_NextPtrs[prev] = m_NextPtrs[i];

                    Volatile.Write(ref m_NextPtrs[i], -1);
                    Interlocked.Decrement(ref *m_Count);
                    return true;
                }
            }

            return false;
        }

        public ParallelReader AsParallelReader() => new ParallelReader
        {
            m_Buckets = m_Buckets,
            m_NextPtrs = m_NextPtrs,
            m_Keys = m_Keys,
            m_Values = m_Values,
            m_States = m_States,
            m_BucketCapacityMask = m_BucketCapacityMask,
            m_Capacity = m_Capacity
        };

        public ParallelWriter AsParallelWriter() => new ParallelWriter
        {
            m_Buckets = m_Buckets,
            m_NextPtrs = m_NextPtrs,
            m_Keys = m_Keys,
            m_Values = m_Values,
            m_States = m_States,
            m_Count = m_Count,
            m_Capacity = m_Capacity,
            m_BucketCapacityMask = m_BucketCapacityMask
        };
       

        public struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction] internal int* m_Buckets;
            [NativeDisableUnsafePtrRestriction] internal int* m_NextPtrs;
            [NativeDisableUnsafePtrRestriction] internal TKey* m_Keys;
            [NativeDisableUnsafePtrRestriction] internal TValue* m_Values;
            [NativeDisableUnsafePtrRestriction] internal int* m_States;
            internal int m_BucketCapacityMask;
            internal int m_Capacity;  
            public TryGetResult TryGetValue(TKey key, out TValue value)
            {
                int hash = key.GetHashCode();
                int bucket = hash & m_BucketCapacityMask;

                int steps = 0;
                int start = Volatile.Read(ref m_Buckets[bucket]);
              //  UnityEngine.Debug.Log($"[TryGetValue] Key={key}, Hash={hash}, Bucket={bucket}, Start={start}");

                for (int i = start; i != -1 && steps++ < 128; i = m_NextPtrs[i])
                {
                    int state = Volatile.Read(ref m_States[i]);
                  //  UnityEngine.Debug.Log($"  -> Step {steps}, i={i}, state={state}, key={m_Keys[i]}");

                    if (state == 2 && m_Keys[i].Equals(key))
                    {
                        value = m_Values[i];
                       // UnityEngine.Debug.Log($"  [Found] key={key}, i={i}, value={value}");
                        return TryGetResult.Found;
                    }

                    if (state == 1 && m_Keys[i].Equals(key))
                    {
                        value = default;
                      //  UnityEngine.Debug.Log($"  [Pending] key={key}, i={i}");
                        return TryGetResult.PendingWrite;
                    }
                }

                // 全局扫描 fallback（可选）
                for (int i = 0; i < m_Capacity; ++i)
                {
                    int state = Volatile.Read(ref m_States[i]);
                    if (state != 0 && m_Keys[i].Equals(key))
                    {
                      //  UnityEngine.Debug.Log($"  [ScanFallback] key={key}, state={state}, i={i}");
                        value = default;
                        return TryGetResult.PendingWrite; 
                    }
                }

               // UnityEngine.Debug.Log($"  [NotFound] key={key}");
                value = default;
                return TryGetResult.NotFound;
            }

        }

        public struct ParallelWriter
        {
            [NativeDisableUnsafePtrRestriction] internal int* m_Buckets;
            [NativeDisableUnsafePtrRestriction] internal int* m_NextPtrs;
            [NativeDisableUnsafePtrRestriction] internal TKey* m_Keys;
            [NativeDisableUnsafePtrRestriction] internal TValue* m_Values;
            [NativeDisableUnsafePtrRestriction] internal int* m_States;
            [NativeDisableUnsafePtrRestriction] internal int* m_Count;
            internal int m_Capacity;
            internal int m_BucketCapacityMask;

            public bool TryAdd(TKey key, TValue value)
            {
                int hash = key.GetHashCode();
                int bucket = hash & m_BucketCapacityMask;

                // 先检查：已有 key 且状态为已完成，直接失败
                for (int i = Volatile.Read(ref m_Buckets[bucket]), steps = 0; i != -1 && steps++ < 128; i = m_NextPtrs[i])
                {
                    int state = Volatile.Read(ref m_States[i]);
                    if (state == 2 && m_Keys[i].Equals(key))
                        return false;
                }

                // 分配插入槽位
                int idx = Interlocked.Increment(ref *m_Count) - 1;
                if (idx >= m_Capacity) return false;

                // 写入 key 
                m_Keys[idx] = key;

                // 标记为“写入中”
                Volatile.Write(ref m_States[idx], 1); // ✅ 写入前显式标记状态 1

                // 写入value
                m_Values[idx] = value;

                // 插入到链表
                int oldHead;
                do
                {
                    oldHead = Volatile.Read(ref m_Buckets[bucket]);
                    if (oldHead == idx) return false;
                    m_NextPtrs[idx] = oldHead;
                } while (Interlocked.CompareExchange(ref m_Buckets[bucket], idx, oldHead) != oldHead);

                // ✅ 完成写入，设置为“完成状态”
                Thread.MemoryBarrier();
                Volatile.Write(ref m_States[idx], 2);

                return true;
            }

            public bool TrySet(TKey key, TValue value)
            {
                int hash = key.GetHashCode();
                int bucket = hash & m_BucketCapacityMask;

                for (int i = Volatile.Read(ref m_Buckets[bucket]), steps = 0; i != -1 && steps++ < 128; i = m_NextPtrs[i])
                {
                    int state = Volatile.Read(ref m_States[i]);
                    if (state == 2 && m_Keys[i].Equals(key))
                    {
                        m_Values[i] = value;
                        return true;
                    }
                }

                return false;
            }


            public bool TryRemove(TKey key)
            {
                int hash = key.GetHashCode();
                int bucket = hash & m_BucketCapacityMask;

                int prev = -1;
                for (int i = Volatile.Read(ref m_Buckets[bucket]), steps = 0; i != -1 && steps++ < 128; prev = i, i = m_NextPtrs[i])
                {
                    if (Volatile.Read(ref m_States[i]) == 2 && m_Keys[i].Equals(key))
                    {
                        if (prev < 0)
                            m_Buckets[bucket] = m_NextPtrs[i];
                        else
                            m_NextPtrs[prev] = m_NextPtrs[i];

                        Volatile.Write(ref m_NextPtrs[i], -1);
                        Interlocked.Decrement(ref *m_Count);
                        return true;
                    }
                }

                return false;
            }
        }
    }
}

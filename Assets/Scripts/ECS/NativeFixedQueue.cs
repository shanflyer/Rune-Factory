using  System;
using System.Runtime.CompilerServices;
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
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public unsafe struct NativeFixedQueue<T> : IDisposable where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private int* m_States;
        [NativeDisableUnsafePtrRestriction]
        private T* m_Data;

        [NativeDisableUnsafePtrRestriction]
        private long* m_Head;
        [NativeDisableUnsafePtrRestriction]
        private long* m_Tail;

        private int m_Capacity;
        private int m_CapacityMask;
        private Allocator m_Allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule]
        private DisposeSentinel m_DisposeSentinel;
#endif

        public NativeFixedQueue(int capacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = CollectionHelper.CreateSafetyHandle(allocator);
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
#endif
            if (!math.ispow2(capacity))
                throw new ArgumentException("Capacity must be a power of 2");

            m_Allocator = allocator;
            m_Capacity = capacity;
            m_CapacityMask = capacity - 1;

            m_States = (int*)UnsafeUtility.Malloc(sizeof(int) * capacity, 4, allocator);
            m_Data = (T*)UnsafeUtility.Malloc(sizeof(T) * capacity, 16, allocator);
            m_Head = (long*)UnsafeUtility.Malloc(sizeof(long), 8, allocator);
            m_Tail = (long*)UnsafeUtility.Malloc(sizeof(long), 8, allocator);

            UnsafeUtility.MemClear(m_States, sizeof(int) * capacity);
            *m_Head = 0L;
            *m_Tail = 0L;
        }

        public bool IsCreated => m_States != null && m_Data != null;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (IsCreated)
            {
                DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
            }
#endif
            if (m_States != null) UnsafeUtility.Free(m_States, m_Allocator);
            if (m_Data != null) UnsafeUtility.Free(m_Data, m_Allocator);
            if (m_Head != null) UnsafeUtility.Free(m_Head, m_Allocator);
            if (m_Tail != null) UnsafeUtility.Free(m_Tail, m_Allocator);

            m_States = null;
            m_Data = null;
            m_Head = null;
            m_Tail = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T item)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            long head = Volatile.Read(ref *m_Head);
            long tail = Volatile.Read(ref *m_Tail);

            if (tail - head >= m_Capacity)
            {
                return false;
            }

            long index = Interlocked.Increment(ref *m_Tail) - 1;
            index &= m_CapacityMask;

            if (Interlocked.CompareExchange(ref m_States[index], 1, 0) == 0)
            {
                UnsafeUtility.CopyStructureToPtr(ref item, m_Data + index);

                Thread.MemoryBarrier();
                Volatile.Write(ref m_States[index], 2); // 2 表示 FULL
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T item)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            long head = Volatile.Read(ref *m_Head);
            while (true)
            {
                long index = head & m_CapacityMask;

                if (Volatile.Read(ref m_States[index]) == 2) // FULL
                {
                    if (Interlocked.CompareExchange(ref *m_Head, head + 1, head) == head)
                    {
                        item = m_Data[index];
                        Thread.MemoryBarrier();
                        Volatile.Write(ref m_States[index], 0); // EMPTY
                        return true;
                    }
                    else
                    {
                        head = Volatile.Read(ref *m_Head); // retry
                        continue;
                    }
                }
                else
                {
                    item = default;
                    return false;
                }
            }
        }

        public long EstimateCount()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            long tail = Volatile.Read(ref *m_Tail);
            long head = Volatile.Read(ref *m_Head);
            return tail - head;
        }

        public bool IsFull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => EstimateCount() >= m_Capacity;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => EstimateCount() == 0;
        }

        public ParallelWriter AsParallelWriter()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(m_Safety);
#endif
            return new ParallelWriter
            {
                m_States = m_States,
                m_Data = m_Data,
                m_Head = m_Head,
                m_Tail = m_Tail,
                m_Capacity = m_Capacity,
                m_CapacityMask = m_CapacityMask
            };
        }

        [NativeContainerIsAtomicWriteOnly]
        [BurstCompile(FloatMode = FloatMode.Fast)]
        public unsafe struct ParallelWriter
        {
            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal int* m_States;

            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal T* m_Data;

            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal long* m_Head;

            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal long* m_Tail;

            internal int m_Capacity;
            internal int m_CapacityMask;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueue(T item)
            {
                long head = Volatile.Read(ref *m_Head);
                long tail = Volatile.Read(ref *m_Tail);

                if (tail - head >= m_Capacity)
                {
                    return false;
                }

                long index = Interlocked.Increment(ref *m_Tail) - 1;
                index &= m_CapacityMask;

                if (Interlocked.CompareExchange(ref m_States[index], 1, 0) == 0)
                {
                    UnsafeUtility.CopyStructureToPtr(ref item, m_Data + index);

                    Thread.MemoryBarrier();
                    Volatile.Write(ref m_States[index], 2);
                    return true;
                }
                return false;
            }
        }

        public ParallelReader AsParallelReader()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(m_Safety);
#endif
            return new ParallelReader
            {
                m_States = m_States,
                m_Data = m_Data,
                m_Head = m_Head,
                m_CapacityMask = m_CapacityMask
            };
        }

        [BurstCompile(FloatMode = FloatMode.Fast)]
        public unsafe struct ParallelReader
        {
            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal int* m_States;

            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal T* m_Data;

            [NativeDisableUnsafePtrRestriction]
            [NativeDisableContainerSafetyRestriction]
            internal long* m_Head;

            internal int m_CapacityMask;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryDequeue(out T item)
            {
                long head = Volatile.Read(ref *m_Head);
                while (true)
                {
                    long index = head & m_CapacityMask;

                    if (Volatile.Read(ref m_States[index]) == 2)
                    {
                        if (Interlocked.CompareExchange(ref *m_Head, head + 1, head) == head)
                        {
                            item = m_Data[index];
                            Thread.MemoryBarrier();
                            Volatile.Write(ref m_States[index], 0);
                            return true;
                        }
                        else
                        {
                            head = Volatile.Read(ref *m_Head);
                            continue;
                        }
                    }
                    else
                    {
                        item = default;
                        return false;
                    }
                }
            }
        }
    }
}

// costo inserimento lineare O(n)
// costo pop costante O(1)

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

[NativeContainerSupportsDeallocateOnJobCompletion]
[NativeContainer]
public unsafe struct NativeMinHeap : IDisposable
{
    private readonly Allocator allocator;
 
    [NativeDisableUnsafePtrRestriction]
    private void* buffer;
 
    private int capacity;
 
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private AtomicSafetyHandle m_Safety;
 
    [NativeSetClassTypeToNullOnSchedule]
    private DisposeSentinel m_DisposeSentinel;
#endif
 
    private int head;
 
    private int length;
 
    public NativeMinHeap(int capacity, Allocator allocator)
    {
        var size = (long)UnsafeUtility.SizeOf<MinHeapNode>() * capacity;
        if (allocator <= Allocator.None)
        {
            throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
        }
 
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Length must be >= 0");
        }
 
        if (size > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), $"Length * sizeof(MinHeapNode) cannot exceed {int.MaxValue} bytes");
        }
 
        buffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<MinHeapNode>(), allocator);
        this.capacity = capacity;
        this.allocator = allocator;
        head = -1;
        length = 0;
 
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 1, allocator);
#endif
    }
    
    public NativeMinHeap Slice(int start, int length)
    {
        var stride = UnsafeUtility.SizeOf<MinHeapNode>();
 
        return new NativeMinHeap
        {
            buffer = (byte*) ((IntPtr) buffer + stride * start),
            capacity = length,
            length = 0,
            head = -1,
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = m_Safety,
#endif
        };
    }
 
    public bool HasNext()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
        return head >= 0;
    }
 
    public void Push(MinHeapNode node)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (length == capacity)
        {
            throw new IndexOutOfRangeException($"Capacity of {capacity} Reached: {length}");
        }
 
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
        if (head < 0)
        {
            head = length;
        }
        else if (node.F_Cost < Get(head).F_Cost ||
                 (node.F_Cost == Get(head).F_Cost && node.H_Cost < Get(head).H_Cost))
        {
            node.Next = head;
            head = length;
        }
        else
        {
            var currentPtr = head;
            var current = Get(head);
 
            while (current.Next >= 0 && Get(current.Next).F_Cost <= node.F_Cost)
            {
                if (node.F_Cost == Get(current.Next).F_Cost && node.H_Cost < Get(current.Next).H_Cost)
                    break;
                
                currentPtr = current.Next;
                current = Get(current.Next);
            }
 
            node.Next = current.Next;
            current.Next = length;
 
            UnsafeUtility.WriteArrayElement(buffer, currentPtr, current);
        }
 
        UnsafeUtility.WriteArrayElement(buffer, length, node);
        this.length += 1;
    }
 
    public MinHeapNode Pop()
    {
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
        #endif
        
        var result = head;
        head = Get(head).Next;
        return Get(result);
    }

    public bool IfContainsRemove(Entity node)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
        if (head < 0)
            return false;
        
        var current = Get(head);
        
        if (current.NodeEntity == node)
        {
            head = current.Next;
            return true;
        }

        if (current.Next == -1)
            return false;
        
        MinHeapNode prev = current;
        current = Get(current.Next);
        var currentPrev = head;
        
        while (current.Next > -1)
        {
            if (current.NodeEntity == node)
            {
                prev.Next = current.Next;
                UnsafeUtility.WriteArrayElement(buffer, currentPrev, prev);
                return true;
            }

            currentPrev = prev.Next;
            prev = current;
            current = Get(current.Next);
        }
        
        if (current.NodeEntity == node)
        {
            prev.Next = -1;
            UnsafeUtility.WriteArrayElement(buffer, currentPrev, prev);
            return true;
        }
        return false;
    }
    
    public bool Contains(Entity node)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
        if (head < 0)
            return false;
        
        var current = Get(head);
        
        while (current.Next > -1)
        {    
            if (current.NodeEntity == node)
            {
                return true;
            }
            current = Get(current.Next);
        }
        if (current.NodeEntity == node)
        {
            return true;
        }
        return false;
    }

    public void Clear()
    {
        head = -1;
        length = 0;
    }
 
    public void Dispose()
    {
        if (!UnsafeUtility.IsValidAllocator(allocator))
        {
            return;
        }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
        UnsafeUtility.Free(buffer, allocator);
        buffer = null;
        capacity = 0;
    }
 
    private MinHeapNode Get(int index)
    {
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (index < 0 || index >= length)
        {
            this.OutOfRangeError(index);
        }
 
        AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
        #endif
 
        return UnsafeUtility.ReadArrayElement<MinHeapNode>(buffer, index);
    }
 
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private void OutOfRangeError(int index)
    {
        throw new IndexOutOfRangeException($"Index {index} is out of range of '{capacity}' range.");
    }
#endif
}

public struct MinHeapNode
{
    public MinHeapNode(Entity nodeEntity, int2 position, int2 parentPosition, int fCost = 0, int hCost = 0)
    {
        NodeEntity = nodeEntity;
        ParentPosition = parentPosition;
        Position = position;
        F_Cost = fCost;
        H_Cost = hCost;
        Next = -1;
        IsClosed = 0;
    }
    
    public MinHeapNode(Entity nodeEntity, int2 position, int fCost = 0, int hCost = 0)
    {
        NodeEntity = nodeEntity;
        ParentPosition = new int2(-1,-1);
        Position = position;
        F_Cost = fCost;
        H_Cost = hCost;
        Next = -1;
        IsClosed = 0;
    }

    public Entity NodeEntity { get; }
    public int2 ParentPosition { get; set; }
    public int2 Position { get; }
    public int F_Cost { get; }
    public int H_Cost { get; }
    public int Next { get; set; }
    public byte IsClosed { get; set; }
}
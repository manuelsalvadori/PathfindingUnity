﻿// costo inserimento lineare O(n)
// costo pop costante O(1)

using System;
using BovineLabs.Systems.Pathfinding;
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
        var size = (long)UnsafeUtility.SizeOf<BovineLabs.Systems.Pathfinding.MinHeapNode>() * capacity;
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
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                $"Length * sizeof(T) cannot exceed {int.MaxValue} bytes");
        }
 
        this.buffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<BovineLabs.Systems.Pathfinding.MinHeapNode>(), allocator);
        this.capacity = capacity;
        this.allocator = allocator;
        this.head = -1;
        this.length = 0;
 
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out this.m_Safety, out this.m_DisposeSentinel, 1, allocator);
#endif
    }
 
    public bool HasNext()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
        return this.head >= 0;
    }
 
    public void Push(MinHeapNode node)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (this.length == this.capacity)
        {
            throw new IndexOutOfRangeException("Capacity Reached");
        }
 
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
        if (head < 0)
        {
            head = this.length;
        }
        else if (node.F_Cost < this.Get(head).F_Cost ||
                 (node.F_Cost == this.Get(head).F_Cost && node.H_Cost < this.Get(head).H_Cost))
        {
            node.Next = head;
            head = length;
        }
        else
        {
            var currentPtr = this.head;
            var current = this.Get(currentPtr);
 
            while (current.Next >= 0 && this.Get(current.Next).F_Cost <= node.F_Cost)
            {
                if (node.F_Cost == this.Get(current.Next).F_Cost && node.H_Cost < this.Get(current.Next).H_Cost)
                    break;
                
                currentPtr = current.Next;
                current = this.Get(current.Next);
            }
 
            node.Next = current.Next;
            current.Next = this.length;
 
            UnsafeUtility.WriteArrayElement(this.buffer, currentPtr, current);
        }
 
        UnsafeUtility.WriteArrayElement(this.buffer, this.length, node);
        this.length += 1;
    }
 
    public MinHeapNode Pop()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(this.m_Safety);
#endif
        var result = this.head;
        this.head = this.Get(this.head).Next;
        return this.Get(result);
    }

    public void Clear()
    {
        this.head = -1;
        this.length = 0;
    }
 
    public void Dispose()
    {
        if (!UnsafeUtility.IsValidAllocator(this.allocator))
        {
            return;
        }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Dispose(ref this.m_Safety, ref this.m_DisposeSentinel);
#endif
        UnsafeUtility.Free(this.buffer, this.allocator);
        this.buffer = null;
        this.capacity = 0;
    }
 
    public NativeMinHeap Slice(int start, int length)
    {
        var stride = UnsafeUtility.SizeOf<BovineLabs.Systems.Pathfinding.MinHeapNode>();
 
        return new NativeMinHeap()
        {
            buffer = (byte*) ((IntPtr) this.buffer + stride * start),
            capacity = length,
            length = 0,
            head = -1,
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = this.m_Safety,
#endif
        };
    }
 
    private MinHeapNode Get(int index)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (index < 0 || index >= this.length)
        {
            this.FailOutOfRangeError(index);
        }
 
        AtomicSafetyHandle.CheckReadAndThrow(this.m_Safety);
#endif
 
        return UnsafeUtility.ReadArrayElement<MinHeapNode>(this.buffer, index);
    }
 
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private void FailOutOfRangeError(int index)
    {
        throw new IndexOutOfRangeException($"Index {index} is out of range of '{this.capacity}' Length.");
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

//public struct MinHeapNode
//{
//    public MinHeapNode(Entity nodeEntity, int2 position, Entity parentEntity, int fCost = 0, int hCost = 0)
//    {
//        NodeEntity = nodeEntity;
//        ParentEntity = parentEntity;
//        Position = position;
//        F_Cost = fCost;
//        H_Cost = hCost;
//        Next = -1;
//    }
//    
//    public MinHeapNode(Entity nodeEntity, int2 position, int fCost = 0, int hCost = 0)
//    {
//        NodeEntity = nodeEntity;
//        ParentEntity = Entity.Null;
//        Position = position;
//        F_Cost = fCost;
//        H_Cost = hCost;
//        Next = -1;
//    }
//
//    public Entity NodeEntity { get; }
//    public Entity ParentEntity { get; set; }
//    public int2 Position { get; }
//    public int F_Cost { get; }
//    public int H_Cost { get; }
//    public int Next { get; set; }
//}
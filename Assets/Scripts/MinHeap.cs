using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

public class MinHeap
{
    private MinHeapNode[] m_Buffer;
    private int m_capacity;

    private int m_head;
    private int m_count;
    private int m_MinIndex;
    private int m_MaxIndex;
 
    public MinHeap(int capacity)
    {
        m_Buffer = new MinHeapNode[capacity];
    }
 
    public bool HasNext()
    {
        return m_head >= 0;
    }
 
    public void Push(MinHeapNode node)
    {
 
        if (m_head < 0)
        {
            m_head = m_count;
        }
        else if (node.F_Cost < this[m_head].F_Cost)
        {
            node.Next = m_head;
            m_head = m_count;
        }
        else
        {
            var currentPtr = m_head;
            var current = this[currentPtr];
 
            while (current.Next >= 0 && node.F_Cost > this[current.Next].F_Cost)
            {                
                currentPtr = current.Next;
                current = this[current.Next];
            }
 
            node.Next = current.Next;
            current.Next = m_count;
 
            m_Buffer[currentPtr] = current;
            
        }
 
        m_Buffer[m_count] = node;
        m_count += 1;
    }
 
    public MinHeapNode Pop()
    {      
        var result = m_head;
        m_head = this[m_head].Next;
        return this[result];
    }
 
    public MinHeapNode this[int index] => m_Buffer[index];

    public bool Contains(Entity node)
    {
        if (m_head == -1)
            return false;
        
        MinHeapNode current = this[m_head];
        while (current.Next >= 0)
        {
            if (current.NodeEntity == node)
                return true;
        }
        return false;
    }

    public void Clear()
    {
        m_head = -1;
        m_count = 0;
    }
}

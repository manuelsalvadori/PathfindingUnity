﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Debug = UnityEngine.Debug;

public class AStarSystem : JobComponentSystem
{   
    private struct AStarJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<TargetInput> targetInput;
        [ReadOnly] public EntityCommandBuffer.Concurrent Commands;
        [ReadOnly] public ComponentDataFromEntity<Walkable> Walkables;
        [ReadOnly] private const int maxLength = 2500;
        [ReadOnly] private const int maxX = 50;
        [ReadOnly] public AgentGroup AgentGroup;
        
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Waypoints> Waypoints;
        public void Execute(int index)
        {  
            int2 start = GridGenerator.ClosestNode(AgentGroup.Position[index].Value);
            int2 goal = GridGenerator.ClosestNode(AgentGroup.Target[index].Value);
            //int2 goal2 = targetInput[0].Value;
            int2 goal2 = start +  new int2(49,0);
            Commands.RemoveComponent<Target>(index, AgentGroup.AgentEntity[index]);
            AStarSolver(start, goal2, index, AgentGroup.AgentEntity[index]);
        }

        private void AStarSolver(int2 start, int2 goal, int index, Entity agent)
        {

            var openSet = new NativeMinHeap(maxLength, Allocator.TempJob);
            var closedSet = new NativeArray<MinHeapNode>(maxLength, Allocator.TempJob);
            var G_Costs = new NativeArray<int>(maxLength, Allocator.TempJob);
            var neighbours = new NativeList<int2>(Allocator.TempJob);

            var startNode = new MinHeapNode(GridGenerator.grid[start.x,start.y], start);
                        
            openSet.Push(startNode);
        
            while (openSet.HasNext())
            {
                var currentNode = openSet.Pop();

                currentNode.IsClosed = 1;
                closedSet[GetIndex(currentNode.Position)] = currentNode;
            
                //Commands.SetSharedComponent(index, currentNode.NodeEntity, Bootstrap.closedLook);

                if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
                {
                    var path = new NativeList<int2>(Allocator.TempJob);
                    var current = currentNode;
                    while(current.ParentPosition.x != -1)
                    {
                        path.Add(current.Position);
                        //Commands.SetSharedComponent(index, current.NodeEntity, Bootstrap.pathLook);
                        current = closedSet[GetIndex(current.ParentPosition)];
                    }
                    path.Add(current.Position);
                    //Commands.SetSharedComponent(index, current.NodeEntity, Bootstrap.pathLook);
                    
                    CreatePath(index, agent, ref path);
                    path.Dispose();
                    break;
                }

                GetNeighbours(currentNode.Position, ref neighbours);

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (closedSet[GetIndex(neighbours[i])].IsClosed == 1)
                        continue;
                
                    int costSoFar = G_Costs[GetIndex(currentNode.Position)] + Heuristics.OctileDistance(currentNode.Position, neighbours[i]);

                    //Commands.SetSharedComponent(index, GridGenerator.grid[neighbours[i].x,neighbours[i].y], Bootstrap.openLook);

                    if (G_Costs[GetIndex(neighbours[i])] == 0 || costSoFar < G_Costs[GetIndex(neighbours[i])])
                    {
                        // update costs
                        int g = costSoFar;
                        int h = Heuristics.OctileDistance(neighbours[i], goal);
                        int f = g + h;
                    
                        G_Costs[GetIndex(neighbours[i])] = g;
 
                        var node = new MinHeapNode(GridGenerator.grid[neighbours[i].x,neighbours[i].y], neighbours[i], currentNode.Position, f, h);
                        openSet.Push(node);
                    }
                }    
            }
            openSet.Dispose();
            closedSet.Dispose();
            G_Costs.Dispose();
            neighbours.Dispose();
        }

        private void CreatePath(int index, Entity agent, ref NativeList<int2> path)
        {
            int2 dir;
            int2 oldDir = new int2(0,0);
            int pathIndex = 0;
            
            DynamicBuffer<int2> waypoints = Waypoints[agent].Reinterpret<int2>();
            waypoints.Clear();
            
            for (int i = 1; i < path.Length; i++)
            {
                dir = path[i - 1] - path[i];
                if (dir.x != oldDir.x || dir.y != oldDir.y)
                {
                    //DisplayPathStep(index, new float3(path[i-1].x, 1f, path[i-1].y));
                    waypoints.Add(path[i - 1]);
                    oldDir = dir;
                }                
            }
        }

        private void DisplayPathStep(int index, float3 stepPos)
        {
            Commands.SetSharedComponent(index, GridGenerator.grid[(int)stepPos.x,(int)stepPos.z], Bootstrap.openLook);
        }

        private void GetNeighbours(int2 coords, ref NativeList<int2> neighbours)
        {   
            neighbours.Clear();
            for (int x = -1; x <= 1; x++)
            {
                var checkX = coords.x + x;
                for (int y = -1; y <= 1; y++)
                {
                    if(x == 0 && y == 0)
                        continue;

                    var checkY = coords.y + y;
                    if (checkX >= 0 && checkX < GridGenerator.grid.GetLength(0) && checkY >= 0 && checkY < GridGenerator.grid.GetLength(1))
                    {
                        Entity checkNode = GridGenerator.grid[coords.x + x, coords.y + y];
                        if(Walkables[checkNode].Value)
                        {
                            neighbours.Add(new int2(checkX,checkY));
                        }
                    }
                }
            }            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(int2 i)
        {
            return (i.y * maxX) + i.x;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jh = new AStarJob()
        {
            Walkables = _walkable,
            Commands = _aStarBarrier.CreateCommandBuffer().ToConcurrent(),
            AgentGroup = _agentGroup,
            Waypoints = GetBufferFromEntity<Waypoints>(),
            targetInput = _targetGroup.target
        }.Schedule(_agentGroup.Length, 1, inputDeps);
        jh.Complete();
        return jh;
    }

    private class AStarBarrier : BarrierSystem {}

    [Inject] private AStarBarrier _aStarBarrier;
    
    private struct GridGroup
    {
        [ReadOnly] public ComponentDataArray<Position> Position;
        [ReadOnly] public ComponentDataArray<Node> Node;
        [ReadOnly] public ComponentDataArray<Cost> Cost;
        [ReadOnly] public ComponentDataArray<Walkable> Walkable;
        [ReadOnly] public EntityArray NodeEntity;
        public readonly int Length;
    }
    
    private struct AgentGroup
    {
        [ReadOnly] public ComponentDataArray<Position> Position;
        [ReadOnly] public ComponentDataArray<Agent> Agent;
        [ReadOnly] public ComponentDataArray<Target> Target;
        [ReadOnly] public EntityArray AgentEntity;
        public readonly int Length;
    }
    
    //debug purpose
    private struct TargetGroup
    {
        [ReadOnly] public ComponentDataArray<TargetInput> target;
        public readonly int Length;
    }

    [Inject] private GridGroup _gridGroup;
    [Inject] private AgentGroup _agentGroup;
    [Inject] private TargetGroup _targetGroup;
    [Inject] private ComponentDataFromEntity<Walkable> _walkable;
}
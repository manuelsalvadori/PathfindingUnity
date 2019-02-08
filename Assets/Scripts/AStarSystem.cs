using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AStarSystem : JobComponentSystem
{   
    private struct AStarJob : IJobParallelFor
    {
        [ReadOnly] public EntityCommandBuffer.Concurrent Commands;
        [ReadOnly] public ComponentDataFromEntity<Walkable> Walkables;
        [ReadOnly] public AgentGroup AgentGroup;
        [ReadOnly] public int2 gridSize;
        [ReadOnly] public int _maxLength;
        [ReadOnly] public int _maxAgents;
        
        [NativeDisableParallelForRestriction] public BufferFromEntity<Waypoints> Waypoints;
        
        public void Execute(int index)
        {
            var start = GridGeneratorSystem.ClosestNode(AgentGroup.Position[index].Value);
            var goal = GridGeneratorSystem.ClosestNode(AgentGroup.Target[index].Value);
            
            Commands.RemoveComponent<ToProcess>(index, AgentGroup.AgentEntity[index]);
            
            AStarSolver(start, goal, index, AgentGroup.AgentEntity[index]);
        }

        private void AStarSolver(int2 start, int2 goal, int index, Entity agent)
        {
            var openSet    = new NativeMinHeap(_maxLength, Allocator.TempJob);
            var closedSet  = new NativeArray<MinHeapNode>(_maxLength, Allocator.TempJob);
            var G_Costs    = new NativeArray<int>(_maxLength, Allocator.TempJob);
            var neighbours = new NativeList<int2>(8, Allocator.TempJob);

            var startNode = new MinHeapNode(GridGeneratorSystem.grid[start.x,start.y], start);
                        
            openSet.Push(startNode);
        
            while (openSet.HasNext())
            {
                var currentNode = openSet.Pop();

                currentNode.IsClosed = 1;
                closedSet[GetIndex(currentNode.Position)] = currentNode;
            
                if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
                {
                    var path = new NativeList<int2>(Allocator.TempJob);
                    var current = currentNode;
                    while(current.ParentPosition.x != -1)
                    {
                        path.Add(current.Position);
                        current = closedSet[GetIndex(current.ParentPosition)];
                    }
                    path.Add(current.Position);
                    
                    CreatePath(index, agent, ref path);
                    path.Dispose();
                    break;
                }

                GetNeighbours(currentNode.Position, ref neighbours);

                for (int i = 0; i < neighbours.Length; i++)
                {
                    var neighbourEntity = GridGeneratorSystem.grid[neighbours[i].x, neighbours[i].y];
                    
                    if (closedSet[GetIndex(neighbours[i])].IsClosed == 1 || !Walkables[neighbourEntity].Value)
                        continue;
                
                    int costSoFar = G_Costs[GetIndex(currentNode.Position)] + Heuristics.OctileDistance(currentNode.Position, neighbours[i]);

                    if (G_Costs[GetIndex(neighbours[i])] == 0 || costSoFar < G_Costs[GetIndex(neighbours[i])])
                    {
                        // update costs
                        int h = Heuristics.OctileDistance(neighbours[i], goal);
                        int f = costSoFar + h;
                    
                        G_Costs[GetIndex(neighbours[i])] = costSoFar;
 
                        var node = new MinHeapNode(neighbourEntity, neighbours[i], currentNode.Position, f, h);
                        
                        // if openSet contains node => update node
                        openSet.IfContainsRemove(node.NodeEntity);
                        openSet.Push(node);                      
                    }
                }    
            }
            openSet.Dispose();
            closedSet .Dispose();
            G_Costs.Dispose();   
            neighbours.Dispose();
        }

        private void CreatePath(int index, Entity agent, ref NativeList<int2> path)
        {
            int2 dir;
            int2 oldDir = new int2(0,0);
            
            DynamicBuffer<int2> waypoints = Waypoints[agent].Reinterpret<int2>();
            waypoints.Clear();
            
            for (int i = 1; i < path.Length; i++)
            {
                dir = path[i - 1] - path[i];
                if (dir.x != oldDir.x || dir.y != oldDir.y)
                {
                    waypoints.Add(path[i - 1]);
                    oldDir = dir;
                }                
            }
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
                    if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                    {
                        neighbours.Add(new int2(checkX,checkY));
                    }
                }
            }            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(int2 i)
        {
            return (i.y * gridSize.x) + i.x;
        }
    }

//    private List<KeyValuePair<float, double>> times = new List<KeyValuePair<float, double>>();

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
//        Stopwatch sw = new Stopwatch();
//        sw.Start();
        
        var job = new AStarJob
        {
            Walkables = GetComponentDataFromEntity<Walkable>(true),
            Commands = _aStarBarrier.CreateCommandBuffer().ToConcurrent(),
            AgentGroup = _agentGroup,
            Waypoints = GetBufferFromEntity<Waypoints>(),
            gridSize = Bootstrap.Settings.gridSize,
            _maxLength = Bootstrap.Settings.gridSize.x * Bootstrap.Settings.gridSize.y
        }.Schedule(_agentGroup.Length, 1, inputDeps);
//        job.Complete();
        
//        sw.Stop();
//        times.Add(new KeyValuePair<float, double>(Time.time, sw.Elapsed.TotalMilliseconds));
        return job;
    }

//    protected override void OnDestroyManager()
//    {
//        string path = $"{Application.persistentDataPath}/aStarDataECS.txt";
//        
//        StreamWriter writer = new StreamWriter(path, true);
//
//        foreach (var pair in times)
//        {
//            writer.WriteLine($"{pair.Key} {pair.Value}");
//        }
//        writer.Close();
//    }

    private class AStarBarrier : BarrierSystem {}
    [Inject] private AStarBarrier _aStarBarrier;
    
    private struct AgentGroup
    {
        [ReadOnly] public ComponentDataArray<Position> Position;
        [ReadOnly] public ComponentDataArray<Agent> Agent;
        [ReadOnly] public ComponentDataArray<ToProcess> ToProcess;
        [ReadOnly] public ComponentDataArray<Target> Target;
        [ReadOnly] public EntityArray AgentEntity;
        public readonly int Length;
    }

    [Inject] private AgentGroup _agentGroup;
}


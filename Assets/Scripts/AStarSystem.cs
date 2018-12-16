using System.Collections;
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
    //[BurstCompile]
    private struct AStarJob : IJobParallelFor//IJobProcessComponentDataWithEntity<Agent, Target>
    {
        [ReadOnly] public EntityCommandBuffer Commands;
        [ReadOnly] public ComponentDataFromEntity<Position> Positions;
        [ReadOnly] public ComponentDataFromEntity<Walkable> Walkables;
        [ReadOnly] private const int maxLength = 2500;
        [ReadOnly] private const int maxX = 50;
        public EntityArray NodeEntityArray;
        public AgentGroup AgentGroup;

        public void Execute(int index)
        {
            int2 start = GridGenerator.ClosestNode(AgentGroup.Position[index].Value);
            int2 goal = GridGenerator.ClosestNode(AgentGroup.Target[index].Value);
            AStarSolver(start, goal);
        }
        
//        public void Execute(Entity agent, int index, ref Agent data, ref Target target)
//        {
//            //Commands.RemoveComponent<Target>(agent);
//            int2 start = GridGenerator.ClosestNode(Positions[agent].Value);
//            int2 goal = GridGenerator.ClosestNode(target.Value);
//            AStarSolver(new int2(0,0), new int2(5,5));
//        }

        private void AStarSolver(int2 start, int2 goal)
        {
//            Stopwatch sw = new Stopwatch();
//            Stopwatch sw2 = new Stopwatch();
//        
//            int c = 0;
//            sw.Start();
            var openSet = new NativeMinHeap(maxLength, Allocator.TempJob);
            var closedSet = new NativeArray<MinHeapNode>(maxLength, Allocator.TempJob);
            var G_Costs = new NativeArray<int>(maxLength, Allocator.TempJob);
            var neighbours = new NativeList<int2>(Allocator.TempJob);
            
            var startNode = new MinHeapNode(GridGenerator.grid[start.x,start.y], start.x, start.y);
            openSet.Push(startNode);
        
            while (openSet.HasNext())
            {
                //c++;
                var currentNode = openSet.Pop();

                currentNode.IsClosed = 1;
                closedSet[GetIndex(currentNode.Position)] = currentNode;
            
                //em.SetSharedComponentData(currentNode.NodeEntity, closedLook);

                if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
                {
                    //Debug.Log("fine");
//                    sw.Stop();
//                    Debug.Log("Iterations: "+ c +"Time: " + sw.ElapsedMilliseconds + "ms");
                    var current = currentNode;
                    while(current.ParentPosition.x != -1)
                    {
                        Commands.SetSharedComponent(current.NodeEntity, Bootstrap.pathLook);
                        current = closedSet[GetIndex(current.ParentPosition)];
                        //CreatePathStep(agent, i, path[i]);
                    }
                    Commands.SetSharedComponent(current.NodeEntity, Bootstrap.pathLook);
                    break;
                }

                GetNeighbours(currentNode.Position, ref neighbours);

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (closedSet[GetIndex(neighbours[i])].IsClosed == 1)
                        continue;
                
                    int costSoFar = G_Costs[GetIndex(currentNode.Position)] + Heuristics.OctileDistance(currentNode.Position, neighbours[i]);

                    //em.SetSharedComponentData(GridGenerator.grid[neighbours[i].x,neighbours[i].y], openLook);

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

        private void CreatePathStep(Entity agent, int index, float3 stepPos)
        {
            Commands.CreateEntity(Bootstrap._pathStepArchetype);
            Commands.SetSharedComponent(new ParentAgent{Value = agent});
            Commands.SetComponent(new PathIndex{Value = index});
            Commands.SetComponent(new PathStep{Value = stepPos});
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
//        return new AStarJob()
//        {
//            NodeEntityArray = _gridGroup.NodeEntity,
//            Positions = _position,
//            Walkables = _walkable,
//            Commands = _aStarBarrier.CreateCommandBuffer()
//        }.Schedule(this, inputDeps);

        JobHandle jh = new AStarJob()
        {
            NodeEntityArray = _gridGroup.NodeEntity,
            Positions = _position,
            Walkables = _walkable,
            Commands = _aStarBarrier.CreateCommandBuffer(),
            AgentGroup = _agentGroup
        }.Schedule(_agentGroup.Length, 2, inputDeps);
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
        [ReadOnly] public EntityArray NodeEntity;
        public readonly int Length;
    }

    [Inject] private GridGroup _gridGroup;
    [Inject] private AgentGroup _agentGroup;
    [Inject] private ComponentDataFromEntity<Position> _position;
    [Inject] private ComponentDataFromEntity<Walkable> _walkable;
}
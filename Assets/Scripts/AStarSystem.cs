using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AStarSystem : JobComponentSystem
{   
    //[BurstCompile]
    private struct AStarJob : IJobParallelFor
    {
        [ReadOnly] public EntityCommandBuffer.Concurrent Commands;
        [ReadOnly] public ComponentDataFromEntity<Walkable> Walkables;
        [ReadOnly] private const int maxLength = 2500;
        [ReadOnly] private const int maxX = 50;
        [ReadOnly] public AgentGroup AgentGroup;
        [ReadOnly] public int2 gridSize;
        
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Waypoints> Waypoints;
        
        public void Execute(int index)
        {  
            int2 start = GridGenerator.ClosestNode(AgentGroup.Position[index].Value);
            int2 goal = GridGenerator.ClosestNode(AgentGroup.Target[index].Value);
            Commands.RemoveComponent<Target>(index, AgentGroup.AgentEntity[index]);
            
            AStarSolver(start, goal, index, AgentGroup.AgentEntity[index]);
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
                    var neighbourEntity = GridGenerator.grid[neighbours[i].x, neighbours[i].y];
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
            closedSet.Dispose();
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
            gridSize = new int2(GridGenerator.grid.GetLength(0), GridGenerator.grid.GetLength(1))
        }.Schedule(_agentGroup.Length, 1, inputDeps);
        //jh.Complete();
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

    [Inject] private GridGroup _gridGroup;
    [Inject] private AgentGroup _agentGroup;
    [Inject] private ComponentDataFromEntity<Walkable> _walkable;
}
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class DebugCosts : MonoBehaviour
{
    private int maxLength = 2500;
    private int maxX = 50;
    private EntityManager em;
    private MeshInstanceRenderer openLook;
    private MeshInstanceRenderer closedLook;
    
    // Update is called once per frame
    void Start()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();
        openLook = Bootstrap.GetLook("openLook");
        closedLook = Bootstrap.GetLook("ClosedLook");
        
        StartCoroutine(AStarSolver(new int2(0,0), new int2(49,49)));
    }
    
    private IEnumerator AStarSolver(int2 start, int2 goal)
    {
        yield return new WaitForSeconds(1);
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        
        int c = 0;
        sw.Start();
        var openSet = new NativeMinHeap(maxLength, Allocator.TempJob);
        var closedSet = new NativeArray<MinHeapNode>(maxLength, Allocator.TempJob);
        var G_Costs = new NativeArray<int>(maxLength, Allocator.TempJob);
        var neighbours = new NativeList<int2>(Allocator.TempJob);
            
        var startNode = new MinHeapNode(GridGenerator.grid[start.x,start.y], start.x, start.y);
        openSet.Push(startNode);
        
        while (openSet.HasNext())
        {
            c++;
            
            var currentNode = openSet.Pop();

            currentNode.IsClosed = 1;
            closedSet[GetIndex(currentNode.Position)] = currentNode;
            
            em.SetSharedComponentData(currentNode.NodeEntity, closedLook);

            if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
            {
                //Debug.Log("fine");
                sw.Stop();
                Debug.Log("Iterations: "+ c +"Time: " + sw.ElapsedMilliseconds + "ms");
                var current = currentNode;
                while(current.ParentPosition.x != -1)
                {                    
                    Debug.Log(closedSet[GetIndex(new int2(0,0))].ParentPosition);
                    
                    em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
                    current = closedSet[GetIndex(current.ParentPosition)];
                    //CreatePathStep(agent, i, path[i]);
                }
                em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
                break;
            }

            GetNeighbours(currentNode.Position, ref neighbours);

            for (int i = 0; i < neighbours.Length; i++)
            {
                if (closedSet[GetIndex(neighbours[i])].IsClosed == 1)
                    continue;
                
                int costSoFar = G_Costs[GetIndex(currentNode.Position)] + Heuristics.OctileDistance(currentNode.Position, neighbours[i]);

                em.SetSharedComponentData(GridGenerator.grid[neighbours[i].x,neighbours[i].y], openLook);

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
        Debug.Log("Fine");
        openSet.Dispose();
        closedSet.Dispose();
        G_Costs.Dispose();
        neighbours.Dispose();
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
                    if(em.GetComponentData<Walkable>(checkNode).Value)
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
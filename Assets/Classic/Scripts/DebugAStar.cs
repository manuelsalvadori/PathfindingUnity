using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public unsafe class DebugAStar : MonoBehaviour
{
    public Dictionary<int2, int3> costs = new Dictionary<int2, int3>();
    private EntityManager em;
    private MeshInstanceRenderer openLook;
    private MeshInstanceRenderer closedLook;
    private int2 start;
    void Start()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();
        openLook = Bootstrap.GetLook("openLook");
        closedLook = Bootstrap.GetLook("ClosedLook");
        start = GridGeneratorSystem.ClosestNode(new float3(-24.6f, 1, -24.6f));
        
        
        var mh = new NativeMinHeap(5, Allocator.Temp);
        mh.Push(new MinHeapNode(new Entity{Index = 0}, new int2(0,0),0,0));
        mh.Push(new MinHeapNode(new Entity{Index = 1}, new int2(0,0),0,1));
        mh.Push(new MinHeapNode(new Entity{Index = 2}, new int2(0,0),0,2));
        mh.Push(new MinHeapNode(new Entity{Index = 3}, new int2(0,0),1,1));
        // OTTIMIZZARE!!!!!!!!!!!!!!!!
        mh.IfContainsRemove(new Entity {Index = 3});
        mh.Push(new MinHeapNode(new Entity{Index = 4}, new int2(0,0),2,1));

        MinHeapNode a = new MinHeapNode();
        while (mh.HasNext())
        {
            a = mh.Pop();
            Debug.Log(a.NodeEntity.Index+ " "+ mh.HasNext());            
        }
        mh.Dispose();

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
            StartCoroutine(AStarSolver(start, start +  new int2(26,49)));
    }

    private IEnumerator AStarSolver(int2 start, int2 goal)
    {
        var maxLength = 2500;
        
        var openSet = new NativeMinHeap(maxLength, Allocator.Persistent);
        var closedSet = new NativeArray<MinHeapNode>(maxLength, Allocator.Persistent);
        var G_Costs = new NativeArray<int>(maxLength, Allocator.Persistent);
        var neighbours = new NativeList<int2>(Allocator.Persistent);

        var startNode = new MinHeapNode(GridGeneratorSystem.grid[start.x,start.y], start);
                        
        openSet.Push(startNode);
        var c = 0;
        while (openSet.HasNext())
        {
            var currentNode = openSet.Pop();

            Debug.Log("Iteration " + c++ + " closed: "+ currentNode.IsClosed + " current: "+ currentNode.Position + " f:" + currentNode.F_Cost +" h:" + currentNode.H_Cost+ " ************************************");
            
            currentNode.IsClosed = 1;
            closedSet[GetIndex(currentNode.Position)] = currentNode;
            
            em.SetSharedComponentData(currentNode.NodeEntity, Bootstrap.closedLook);

            if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
            {
                var path = new NativeList<int2>(Allocator.TempJob);
                var current = currentNode;
                while(current.ParentPosition.x != -1)
                {
                    path.Add(current.Position);
                    em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
                    current = closedSet[GetIndex(current.ParentPosition)];
                }
                path.Add(current.Position);
                em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
                    
                path.Dispose();
                break;
            }

            GetNeighbours(currentNode.Position, ref neighbours);

            for (int i = 0; i < neighbours.Length; i++)
            {                
                var neighbourEntity = GridGeneratorSystem.grid[neighbours[i].x, neighbours[i].y];
                if (closedSet[GetIndex(neighbours[i])].IsClosed == 1 || !em.GetComponentData<Walkable>(neighbourEntity).Value)
                    continue;
                
                int costSoFar = G_Costs[GetIndex(currentNode.Position)] + Heuristics.OctileDistance(currentNode.Position, neighbours[i]);

                em.SetSharedComponentData(GridGeneratorSystem.grid[neighbours[i].x,neighbours[i].y], Bootstrap.openLook);

                if (G_Costs[GetIndex(neighbours[i])] == 0 || costSoFar < G_Costs[GetIndex(neighbours[i])])
                {
                    // update costs
                    int h = Heuristics.OctileDistance(neighbours[i], goal);
                    int f = costSoFar + h;
                    G_Costs[GetIndex(neighbours[i])] = costSoFar;
 
                    var node = new MinHeapNode(neighbourEntity, neighbours[i], currentNode.Position, f, h);
                    // if contains => update
                    Debug.Log("current neighbour: "+node.Position);
                    if(openSet.IfContainsRemove(node.NodeEntity))
                        Debug.Log("OpenSet: Updating " + node.Position);
                    if(node.Position.x == 2 && node.Position.y == 15)
                        Debug.Log("PUSH 2,15");
                    openSet.Push(node);
                }
            }
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
//           yield return null;
        }
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
                if (checkX >= 0 && checkX < 50 && checkY >= 0 && checkY < 50)
                {
                    neighbours.Add(new int2(checkX,checkY));
                }
            }
        }            
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int2 i)
    {
        return (i.y * 50) + i.x;
    }
}
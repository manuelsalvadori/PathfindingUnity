//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Rendering;
//using UnityEngine;
//using Debug = UnityEngine.Debug;
//
//public class ClassicAStar : MonoBehaviour
//{
//    private Dictionary<int2, int> G_Costs = new Dictionary<int2, int>();
//    private EntityManager em;
//    private MeshInstanceRenderer openLook;
//    private MeshInstanceRenderer closedLook;
//    private const int maxOpenSetLength = 2500;
//    
//    void Start()
//    {
//        em = World.Active.GetOrCreateManager<EntityManager>();
//        openLook = Bootstrap.GetLook("openLook");
//        closedLook = Bootstrap.GetLook("ClosedLook");
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
////        if (Input.GetKeyDown(KeyCode.T))
////        {
//            G_Costs.Clear();
//            AStarSolver(new int2(0, 0), new int2(49, 49));
//        //}
//    }
//
//    private void AStarSolver(int2 start, int2 goal)
//    {
//        Stopwatch sw = new Stopwatch();
//        Stopwatch sw2 = new Stopwatch();
//        
//        int c = 0;
//        sw.Start();
//        var openSet = new NativeMinHeap(maxOpenSetLength, Allocator.Persistent);
//        var closedSet = new Dictionary<Entity, MinHeapNode>();
//        var neighbours = new NativeList<int2>(Allocator.TempJob);
//            
//        var startNode = new MinHeapNode(GridGenerator.grid[start.x,start.y], start.x, start.y);
//        openSet.Push(startNode);
//        
//        while (openSet.HasNext())
//        {
//            c++;
//            var currentNode = openSet.Pop();
//            
//            if(!G_Costs.ContainsKey(currentNode.Position))
//                G_Costs.Add(currentNode.Position, 0);
//            
//            if(!closedSet.ContainsKey(currentNode.NodeEntity))
//                closedSet.Add(currentNode.NodeEntity, currentNode);
//            
//            //em.SetSharedComponentData(currentNode.NodeEntity, closedLook);
//
//            if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
//            {
//                //Debug.Log("fine");
//                sw.Stop();
//                Debug.Log("Iterations: "+ c +"Time: " + sw.ElapsedMilliseconds + "ms");
//                var current = currentNode;
//                while(current.ParentEntity != Entity.Null)
//                {
//                    em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
//                    current = closedSet[current.ParentEntity];
//                    //CreatePathStep(agent, i, path[i]);
//                }
//                em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
//                break;
//            }
//
//            GetNeighbours(currentNode.Position, ref neighbours);
//
//            for (int i = 0; i < neighbours.Length; i++)
//            {
//                if (closedSet.ContainsKey(GridGenerator.grid[neighbours[i].x,neighbours[i].y]))
//                    continue;
//                
//                if(!G_Costs.ContainsKey(neighbours[i]))
//                    G_Costs.Add(neighbours[i], 0);
//                
//                int costSoFar = G_Costs[currentNode.Position] + Heuristics.OctileDistance(currentNode.Position, neighbours[i]);
//
//                //em.SetSharedComponentData(GridGenerator.grid[neighbours[i].x,neighbours[i].y], openLook);
//
//                if (G_Costs[neighbours[i]] == 0 || costSoFar < G_Costs[neighbours[i]])
//                {
//                    // update costs
//                    
//                    int g = costSoFar;
//                    int h = Heuristics.OctileDistance(neighbours[i], goal);
//                    int f = g + h;
//                    
//                    G_Costs[neighbours[i]] = g;
// 
//                    var node = new MinHeapNode(GridGenerator.grid[neighbours[i].x,neighbours[i].y], neighbours[i], currentNode.NodeEntity, f, h);
//                    openSet.Push(node);
//                }
//            }    
//        }
//        Debug.Log("Fine fail");
//        openSet.Dispose();
//        neighbours.Dispose();
//    }
//
//    private void GetNeighbours(int2 coords, ref NativeList<int2> neighbours)
//    {   
//        neighbours.Clear();
//        for (int x = -1; x <= 1; x++)
//        {
//            var checkX = coords.x + x;
//            for (int y = -1; y <= 1; y++)
//            {
//                if(x == 0 && y == 0)
//                    continue;
//
//                var checkY = coords.y + y;
//                if (checkX >= 0 && checkX < GridGenerator.grid.GetLength(0) && checkY >= 0 && checkY < GridGenerator.grid.GetLength(1))
//                {
//                    Entity checkNode = GridGenerator.grid[coords.x + x, coords.y + y];
//                    if(em.GetComponentData<Walkable>(checkNode).Value)
//                    {
//                        neighbours.Add(new int2(checkX,checkY));
//                    }                    
//                }                    
//            }
//        }            
//    }
//}
//

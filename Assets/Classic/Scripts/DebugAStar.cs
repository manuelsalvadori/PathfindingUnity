using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class DebugAStar : MonoBehaviour
{
    public Dictionary<int2, int3> costs = new Dictionary<int2, int3>();
    private EntityManager em;
    private MeshInstanceRenderer openLook;
    private MeshInstanceRenderer closedLook;
    void Start()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();
        openLook = Bootstrap.GetLook("openLook");
        closedLook = Bootstrap.GetLook("ClosedLook");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
            StartCoroutine(AStarSolver(new int2(14,12), new int2(9,19)));
    }

    private IEnumerator AStarSolver(int2 start, int2 goal)
    {  
        var openSet = new List<DebugNode>();
        var closedSet = new Dictionary<Entity, DebugNode>();
            
        var startNode = new DebugNode(GridGenerator.grid[start.x,start.y], start.x, start.y);
        openSet.Add(startNode);
        int c = 0;
        
        while (openSet.Count > 0)
        {
            var currentNode = openSet[0];
            Debug.Log("####################################################### openset.count: "+openSet.Count);
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].F_Cost < currentNode.F_Cost)
                {
                    currentNode = openSet[i];
                }
                Debug.Log("************************** currentBest: " + currentNode.F_Cost + " openSet["+i+"] " + openSet[i].F_Cost + " pos: " + openSet[i].X+ "," + openSet[i].Y);
            }
            
            if(!costs.ContainsKey(new int2(currentNode.X, currentNode.Y)))
                costs.Add(new int2(currentNode.X, currentNode.Y), new int3(0,0,0));
            
            Debug.Log("While " + ++c + " current: " + currentNode.X + "," + currentNode.Y + " currentCosts: " + costs[new int2(currentNode.X, currentNode.Y)]); 
            //Debug.Log("A* current: " + currentNode.X + "," + currentNode.Y);

            openSet.Remove(currentNode);                
            closedSet.Add(currentNode.NodeEntity, currentNode);
            em.SetSharedComponentData(currentNode.NodeEntity, closedLook);

            if (currentNode.X == goal.x && currentNode.Y == goal.y)
            {
                //Debug.Log("fine");
                var current = currentNode;
                while(current.ParentEntity != Entity.Null)
                {
                    em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
                    current = closedSet[current.ParentEntity];
                    //CreatePathStep(agent, i, path[i]);
                }
                em.SetSharedComponentData(current.NodeEntity, Bootstrap.pathLook);
                break;
            }

            List<DebugNode> neighbours = GetNeighbours(currentNode.X, currentNode.Y, ref openSet);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (closedSet.ContainsKey(neighbours[i].NodeEntity))
                    continue;
                
                int costSoFar = costs[new int2(currentNode.X, currentNode.Y)].y +
                                Heuristics.OctileDistance(new int2(currentNode.X, currentNode.Y),
                                    new int2(neighbours[i].X, neighbours[i].Y));
                    
                bool inOpenSet = false;
                foreach (var node in openSet)
                {
                    if (node.X == neighbours[i].X && node.Y == neighbours[i].Y)
                    {
                        inOpenSet = true;
                        break;
                    }
                }
                em.SetSharedComponentData(neighbours[i].NodeEntity, openLook);

                if(!costs.ContainsKey(new int2(neighbours[i].X, neighbours[i].Y)))
                    costs.Add(new int2(neighbours[i].X, neighbours[i].Y), new int3(0,0,0));
                
                Debug.Log("for" + i + ": " + neighbours[i].X + "," + neighbours[i].Y + " " + inOpenSet + " costSoFar: " + costSoFar + " Gcost: " + costs[new int2(neighbours[i].X,neighbours[i].Y)].y);
                if (!inOpenSet || costSoFar < costs[new int2(neighbours[i].X, neighbours[i].Y)].y)
                {
                    if(inOpenSet)
                        Debug.Log("Update G cost");
                    // update costs
                    int3 neibCosts = costs[new int2(neighbours[i].X,neighbours[i].Y)];
                    neibCosts.y = costSoFar;
                    neibCosts.z = Heuristics.OctileDistance(new int2(neighbours[i].X, neighbours[i].Y), goal);
                    neibCosts.x = neibCosts.y + neibCosts.z;
                    costs[new int2(neighbours[i].X, neighbours[i].Y)] = neibCosts;
 
                    neighbours[i].F_Cost = neibCosts.x;
                    neighbours[i].ParentEntity = currentNode.NodeEntity;
                    //Debug.Log("n: " + neighbour.X + "," + neighbour.Y +" g:" + neighbour.G_Cost +" h:" + neighbour.H_Cost);
                    
                    if(!inOpenSet)
                        openSet.Add(neighbours[i]);
                }
            }

            foreach (DebugNode m in openSet)
            {
                //Debug.Log("Openlist: " + m.X + "," + m.Y + " " + m.NodeEntity);
            }
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));   
        }
        Debug.Log("Fine fail");
    }

    private List<DebugNode> GetNeighbours(int xCoord, int yCoord, ref List<DebugNode> openSet)
    {   
        var neighbours = new List<DebugNode>();
        for (int x = -1; x <= 1; x++)
        {
            var checkX = xCoord + x;
            for (int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0)
                    continue;

                var checkY = yCoord + y;
                if (checkX >= 0 && checkX < GridGenerator.grid.GetLength(0) && checkY >= 0 && checkY < GridGenerator.grid.GetLength(1))
                {
                    Entity checkNode = GridGenerator.grid[xCoord + x, yCoord + y];
                    if(em.GetComponentData<Walkable>(checkNode).Value)
                    {
                        var mhn = new DebugNode(checkNode, xCoord + x, yCoord + y);
                        foreach (var node in openSet)
                        {
                            if (node.X == checkX && node.Y == checkY)
                                mhn = node;
                        }
                        neighbours.Add(mhn);
                    }                    
                }                    
            }
        }            
        return neighbours;
    }
    
    public class DebugNode
    {
        public DebugNode(Entity nodeEntity, int x, int y)
        {
            NodeEntity = nodeEntity;
            ParentEntity = Entity.Null;
            X = x;
            Y = y;
            F_Cost = 0;
            Next = -1;
        }
 
        public Entity NodeEntity { get; }
        public Entity ParentEntity { get; set; }
        public int X { get; }
        public int Y { get; }
        public int F_Cost { get; set; }
        public int Next { get; set; }
    }
}


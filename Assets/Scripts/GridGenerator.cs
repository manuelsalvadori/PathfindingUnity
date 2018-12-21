using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int2 gridSize;
    public float nodeSize;
    private static float ns;
    public static Entity[,] grid;
    //public static int walkableNodesCount;
    private MeshInstanceRenderer unwalkableLook;
    public static MeshInstanceRenderer nodeLook;
    private EntityManager entityManager;

    void Start()
    {
        grid = new Entity[gridSize.x,gridSize.y];
        ns = nodeSize;
        //walkableNodesCount = 0;
        
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        Entity node;
        nodeLook = Bootstrap.GetLook("NodeLook");
        unwalkableLook = Bootstrap.GetLook("UnwalkableNodeLook");
        
        for (int x = 0; x < gridSize.x; x++)
        {
            float xPos = GridToWorldPosX(x);
            
            for (int y = 0; y < gridSize.y; y++)
            {
                float yPos = GridToWorldPosY(y);
                
                node = entityManager.CreateEntity(Bootstrap._nodeArchetype);
                entityManager.SetComponentData(node, new Position { Value = new float3(xPos, 0, yPos) });
                
                if (Physics.CheckBox(new Vector3(xPos, 0, yPos), Vector3.one * (nodeSize / 2f)))
                {
                    entityManager.AddComponentData(node, new Walkable { Value = false});
                    entityManager.SetSharedComponentData(node, unwalkableLook);
                }
                else
                {
                    entityManager.AddComponentData(node, new Walkable { Value = true});
                    entityManager.SetSharedComponentData(node, nodeLook);
                    //walkableNodesCount++;
                }

                grid[x, y] = node;                
            }
        }
    }

    public static int2 ClosestNode(float3 pos)
    {
        //return new int2(math.clamp(Mathf.FloorToInt(pos.x), 0 , 49), math.clamp(Mathf.FloorToInt(pos.z), 0 , 49));
        return new int2(math.clamp(Mathf.FloorToInt(pos.x) + 25, 0 , 49), math.clamp(Mathf.FloorToInt(pos.z) + 25, 0 , 49));
    }

    public static float GridToWorldPosX(int coord)
    {
        return coord - (grid.GetLength(0) / 2f) + (ns / 2f);
    }

    public static float GridToWorldPosY(int coord)
    {
        return coord - (grid.GetLength(1) / 2f) + (ns / 2f);
    }
    
    public static int WorldPosToGridX(float pos)
    {
        return (int) (pos + (grid.GetLength(0) / 2f) - (ns / 2f));
    }
    
    public static int WorldPosToGridY(float pos)
    {
        return (int) (pos + (grid.GetLength(1) / 2f) - (ns / 2f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.y) * nodeSize);
    }
}

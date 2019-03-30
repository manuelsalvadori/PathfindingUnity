using System.Collections.Generic;
using RVO;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;


public class GridGeneratorSystem : JobComponentSystem
{
    [ReadOnly] public static int2 s_gridSize;
    private static float nodeSize = 1;
    public static Entity[,] grid;
    
    private MeshInstanceRenderer unwalkableLook;
    public static MeshInstanceRenderer nodeLook;
    
    private EntityManager entityManager;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        s_gridSize = Bootstrap.Settings.gridSize;
        grid = new Entity[s_gridSize.x,s_gridSize.y];
        
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        Entity node;
        nodeLook = Bootstrap.GetLook("NodeLook");
        unwalkableLook = Bootstrap.GetLook("UnwalkableNodeLook");
        
        for (int x = 0; x < s_gridSize.x; x++)
        {
            float xPos = GridToWorldPosX(x);
            
            for (int y = 0; y < s_gridSize.y; y++)
            {
                float yPos = GridToWorldPosY(y);
                
                node = entityManager.CreateEntity(Bootstrap._nodeArchetype);
                entityManager.SetComponentData(node, new Position { Value = new float3(xPos, 0, yPos) });
                entityManager.AddComponentData(node, new Walkable
                {
                    Value = !Physics.CheckBox(new Vector3(xPos, 0, yPos), Vector3.one * (nodeSize / 2f))
                });

                grid[x, y] = node;                
            }
        }
        
        // processing obstacles
        var obstacles = Object.FindObjectsOfType<BoxCollider>();

        foreach(var obst in obstacles)
        {
            var go = obst.gameObject;
            var center = new float2(go.transform.position.x, go.transform.position.z);
            var vertices = new List<float2>();
            vertices.Add(center + new float2(-go.transform.lossyScale.x, -go.transform.lossyScale.z) * 0.5f);
            vertices.Add(center + new float2(go.transform.lossyScale.x, -go.transform.lossyScale.z) * 0.5f);
            vertices.Add(center + new float2(go.transform.lossyScale.x, go.transform.lossyScale.z) * 0.5f);
            vertices.Add(center + new float2(-go.transform.lossyScale.x, go.transform.lossyScale.z) * 0.5f);

            Simulator.Instance.addObstacle(vertices);
        }
        Simulator.Instance.processObstacles();
    }

    public static int2 ClosestNode(float3 pos)
    {
        //return new int2(math.clamp(Mathf.FloorToInt(pos.x), 0 , 49), math.clamp(Mathf.FloorToInt(pos.z), 0 , 49));
        return new int2(math.clamp(Mathf.FloorToInt(pos.x) + s_gridSize.x/2, 0 , s_gridSize.x), math.clamp(Mathf.FloorToInt(pos.z) + s_gridSize.y/2, 0 , s_gridSize.y));
    }
    
    public static float2 GridToWorldPos(int2 coord)
    {
        var x = coord.x - (grid.GetLength(0) / 2f) + (nodeSize / 2f);
        var y = coord.y - (grid.GetLength(1) / 2f) + (nodeSize / 2f);
        return new float2(x,y);
    }

    public static float GridToWorldPosX(int coord)
    {
        return coord - (grid.GetLength(0) / 2f) + (nodeSize / 2f);
    }

    public static float GridToWorldPosY(int coord)
    {
        return coord - (grid.GetLength(1) / 2f) + (nodeSize / 2f);
    }
    
    public static int WorldPosToGridX(float pos)
    {
        return (int) (pos + (grid.GetLength(0) / 2f) - (nodeSize / 2f));
    }
    
    public static int WorldPosToGridY(float pos)
    {
        return (int) (pos + (grid.GetLength(1) / 2f) - (nodeSize / 2f));
    }
}

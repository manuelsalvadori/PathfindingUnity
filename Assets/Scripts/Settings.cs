using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("Max speed an agent can reach")]
    public int maxAgentSpeed = 70;
    
    [Header("Max number of agents present at the same time")]
    public int agentsLimit = 7000;
    
    [Header("Number of new agents to be spawn at the same time")]
    public int newAgents = 50;
    
    [Header("Size of the pathfinding grid")]
    public int2 gridSize = new int2(50, 50);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridSize.x, 0, gridSize.y));
    }
}
